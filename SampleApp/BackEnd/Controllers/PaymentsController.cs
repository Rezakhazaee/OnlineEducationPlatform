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
    public async Task<List<Payment>> Get()
    {
        return await _context.Payments.ToListAsync();
    }


    // POST: api/Payments
    [HttpPost]
    public async Task<ActionResult<Payment>> Create(Payment payment)
    {
        _context.Payments.Add(payment);

        await _context.SaveChangesAsync();

        return payment;
    }
}