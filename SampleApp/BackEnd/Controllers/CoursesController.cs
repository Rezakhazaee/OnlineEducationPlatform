using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using BackEnd.Models;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }


    // GET: api/Courses
    [HttpGet]
    public async Task<List<Course>> Get()
    {
        return await _context.Courses.ToListAsync();
    }


    // POST: api/Courses
    [HttpPost]
    public async Task<ActionResult<Course>> Create(Course course)
    {
        _context.Courses.Add(course);

        await _context.SaveChangesAsync();

        return course;
    }
}