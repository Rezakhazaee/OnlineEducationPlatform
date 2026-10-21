using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class InstallmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InstallmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var today = DateTime.Today;

        var installments = await _context.Installments
            .AsNoTracking()
            .Include(i => i.Enrollment)
                .ThenInclude(e => e!.Student)
            .Include(i => i.Enrollment)
                .ThenInclude(e => e!.Course)
            .Include(i => i.Payments)
            .OrderBy(i => i.DueDate)
            .ThenBy(i => i.InstallmentNumber)
            .Select(i => new
            {
                i.Id,
                i.EnrollmentId,
                i.InstallmentNumber,
                i.Amount,
                i.DueDate,
                i.Status,
                StudentName = i.Enrollment!.Student!.FirstName + " " +
                              i.Enrollment.Student.LastName,
                CourseTitle = i.Enrollment.Course!.Title,
                PaidAmount = i.Payments
                    .Where(p => p.Status == "Paid")
                    .Sum(p => (decimal?)p.Amount) ?? 0,
                CalculatedStatus =
                    i.Status == "Cancelled"
                        ? "Cancelled"
                        : (i.Status == "Paid" ||
                           i.Payments.Any(p => p.Status == "Paid" &&
                                                p.Amount >= i.Amount))
                            ? "Paid"
                            : i.DueDate.Date < today
                                ? "Overdue"
                                : "Pending"
            })
            .ToListAsync();

        return Ok(installments);
    }
}
