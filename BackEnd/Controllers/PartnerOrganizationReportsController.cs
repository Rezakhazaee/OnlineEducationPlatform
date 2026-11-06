using System.Security.Claims;
using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "PartnerOrganization")]
public class PartnerOrganizationReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PartnerOrganizationReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var partnerOrganizationId = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId &&
                        u.Role == "PartnerOrganization" &&
                        u.IsActive &&
                        u.PartnerOrganization != null &&
                        u.PartnerOrganization.IsActive)
            .Select(u => u.PartnerOrganizationId)
            .SingleOrDefaultAsync();

        if (!partnerOrganizationId.HasValue)
            return Unauthorized();

        var enrollments = _context.Enrollments
            .AsNoTracking()
            .Where(e => e.CoursePartnerOrganization != null &&
                        e.CoursePartnerOrganization.PartnerOrganizationId == partnerOrganizationId.Value);

        var enrollmentIds = enrollments.Select(e => e.Id);
        var totalEnrollments = await enrollments.CountAsync();
        var totalStudents = await enrollments.Select(e => e.StudentId).Distinct().CountAsync();
        var totalCourses = await enrollments.Select(e => e.CourseId).Distinct().CountAsync();
        var expectedAmount = await enrollments.SumAsync(e => (decimal?)e.FinalAmount) ?? 0m;

        var totalPaid = await _context.Payments
            .AsNoTracking()
            .Where(p => enrollmentIds.Contains(p.EnrollmentId) && p.Status == "Paid")
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var statuses = await enrollments
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            TotalEnrollments = totalEnrollments,
            TotalStudents = totalStudents,
            TotalCourses = totalCourses,
            ExpectedAmount = expectedAmount,
            TotalPaid = totalPaid,
            RemainingAmount = expectedAmount - totalPaid,
            Statuses = statuses
        });
    }
}
