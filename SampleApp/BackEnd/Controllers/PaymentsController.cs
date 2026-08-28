using BackEnd.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using BackEnd.Models;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }


    // GET: api/Payments
    [HttpGet]
public async Task<List<PaymentDto>> Get()
{
    return await _context.Payments
        .Select(p => new PaymentDto
        {
            Id = p.Id,
            EnrollmentId = p.EnrollmentId,
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            PaymentType = p.PaymentType,
            Description = p.Description,
            Status = p.Status
        })
        .ToListAsync();
}


    // POST: api/Payments
    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto dto)
    {
        var payment = new Payment
{
    EnrollmentId = dto.EnrollmentId,
    Amount = dto.Amount,
    PaymentDate = dto.PaymentDate,
    PaymentType = dto.PaymentType,
    Description = dto.Description,
    Status = dto.Status
};

_context.Payments.Add(payment);

await _context.SaveChangesAsync();


var result = new PaymentDto
{
    Id = payment.Id,
    EnrollmentId = payment.EnrollmentId,
    Amount = payment.Amount,
    PaymentDate = payment.PaymentDate,
    PaymentType = payment.PaymentType,
    Description = payment.Description,
    Status = payment.Status
};

return result;
    }
}