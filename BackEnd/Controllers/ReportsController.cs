using BackEnd.Data;
using BackEnd.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ReportsSummaryDto>> GetSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? courseId,
        [FromQuery] int? instructorId,
        [FromQuery] string? deliveryType,
        [FromQuery] string? status)
    {
        var enrollmentQuery = _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .AsQueryable();

        if (from.HasValue)
        {
            enrollmentQuery = enrollmentQuery
                .Where(e => e.StartDate >= from.Value);
        }

        if (to.HasValue)
        {
            var endDate = to.Value.Date.AddDays(1);

            enrollmentQuery = enrollmentQuery
                .Where(e => e.StartDate < endDate);
        }

        if (courseId.HasValue)
        {
            enrollmentQuery = enrollmentQuery
                .Where(e => e.CourseId == courseId.Value);
        }

        if (instructorId.HasValue)
        {
            enrollmentQuery = enrollmentQuery
                .Where(e => e.InstructorId == instructorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(deliveryType))
        {
            enrollmentQuery = enrollmentQuery
                .Where(e => e.Course != null &&
                            e.Course.DeliveryType == deliveryType);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            enrollmentQuery = enrollmentQuery
                .Where(e => e.Status == status);
        }

        var enrollments = await enrollmentQuery
            .Select(e => new
            {
                e.Id,
                e.StudentId,
                e.CourseId
            })
            .ToListAsync();

        var enrollmentIds = enrollments
            .Select(e => e.Id)
            .ToList();

        decimal totalPaid = 0;
        decimal totalPending = 0;
        decimal totalCancelled = 0;

        if (enrollmentIds.Count > 0)
        {
            var paymentTotals = await _context.Payments
                .AsNoTracking()
                .Where(p => enrollmentIds.Contains(p.EnrollmentId))
                .GroupBy(p => p.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Total = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            totalPaid = paymentTotals
                .Where(x => x.Status == "Paid")
                .Select(x => x.Total)
                .FirstOrDefault();

            totalPending = paymentTotals
                .Where(x => x.Status == "Pending")
                .Select(x => x.Total)
                .FirstOrDefault();

            totalCancelled = paymentTotals
                .Where(x => x.Status == "Cancelled")
                .Select(x => x.Total)
                .FirstOrDefault();
        }

        int totalStudents;
        int totalCourses;

        bool hasFilters =
            from.HasValue ||
            to.HasValue ||
            courseId.HasValue ||
            instructorId.HasValue ||
            !string.IsNullOrWhiteSpace(deliveryType) ||
            !string.IsNullOrWhiteSpace(status);

        if (!hasFilters)
        {
            totalStudents = await _context.Students.CountAsync();
            totalCourses = await _context.Courses.CountAsync();
        }
        else
        {
            totalStudents = enrollments
                .Select(e => e.StudentId)
                .Distinct()
                .Count();

            totalCourses = enrollments
                .Select(e => e.CourseId)
                .Distinct()
                .Count();
        }

        var result = new ReportsSummaryDto
        {
            TotalStudents = totalStudents,
            TotalCourses = totalCourses,
            TotalEnrollments = enrollments.Count,
            TotalPaid = totalPaid,
            TotalPending = totalPending,
            TotalCancelled = totalCancelled
        };

        return Ok(result);
    }
}
