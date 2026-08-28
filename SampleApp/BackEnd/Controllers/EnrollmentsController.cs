using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using BackEnd.Models;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EnrollmentsController(ApplicationDbContext context)
    {
        _context = context;
    }


    // GET: api/Enrollments
    [HttpGet]
    public async Task<List<Enrollment>> Get()
    {
        return await _context.Enrollments.ToListAsync();
    }


    // POST: api/Enrollments
    [HttpPost]
    public async Task<ActionResult<Enrollment>> Create(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);

        await _context.SaveChangesAsync();

        return enrollment;
    }
}