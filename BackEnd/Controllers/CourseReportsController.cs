using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/CourseReports")]
[Authorize(Roles = "Admin")]
public class CourseReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CourseReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseReports()
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Include(e => e.CoursePartnerOrganization)
            .Where(e => e.Status != "Cancelled")
            .ToListAsync();

        var enrollmentIds = enrollments
            .Select(e => e.Id)
            .ToList();

        var paidByEnrollment = await _context.Payments
            .Where(p =>
                enrollmentIds.Contains(p.EnrollmentId) &&
                p.Status == "Paid")
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new
            {
                EnrollmentId = g.Key,
                TotalPaid = g.Sum(p => p.Amount)
            })
            .ToDictionaryAsync(
                x => x.EnrollmentId,
                x => x.TotalPaid);

        var reports = enrollments
            .Where(e => e.Course != null)
            .GroupBy(e => new
            {
                e.CourseId,
                CourseTitle = e.Course!.Title
            })
            .Select(group =>
            {
                var rows = group.ToList();

                var expectedAmount = rows.Sum(e =>
                    e.CoursePartnerOrganization?.AgreedPrice
                    ?? e.Course!.Price);

                var totalPaid = rows.Sum(e =>
                    paidByEnrollment.TryGetValue(
                        e.Id,
                        out var paid)
                        ? paid
                        : 0);

                return new
                {
                    courseId = group.Key.CourseId,
                    courseTitle = group.Key.CourseTitle,
                    enrollmentCount = rows.Count,
                    expectedAmount,
                    totalPaid,
                    remainingAmount = Math.Max(
                        expectedAmount - totalPaid,
                        0)
                };
            })
            .OrderByDescending(x => x.enrollmentCount)
            .ThenBy(x => x.courseTitle)
            .ToList();

        return Ok(reports);
    }
}
