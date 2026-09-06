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
    public async Task<ActionResult<ReportsSummaryDto>> GetSummary()
    {
        var result = new ReportsSummaryDto
        {
            TotalStudents = await _context.Students.CountAsync(),

            TotalCourses = await _context.Courses.CountAsync(),

            TotalEnrollments = await _context.Enrollments.CountAsync(),

            TotalPaid = await _context.Payments
                .Where(p => p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0,

            TotalPending = await _context.Payments
                .Where(p => p.Status == "Pending")
                .SumAsync(p => (decimal?)p.Amount) ?? 0,

            TotalCancelled = await _context.Payments
                .Where(p => p.Status == "Cancelled")
                .SumAsync(p => (decimal?)p.Amount) ?? 0
        };

        return Ok(result);
    }
}