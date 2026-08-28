using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using BackEnd.Models;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }


    // GET: api/Students
    [HttpGet]
    public async Task<List<Student>> Get()
    {
        return await _context.Students.ToListAsync();
    }


    // POST: api/Students
    [HttpPost]
    public async Task<ActionResult<Student>> Create(Student student)
    {
        _context.Students.Add(student);

        await _context.SaveChangesAsync();

        return student;
    }
}