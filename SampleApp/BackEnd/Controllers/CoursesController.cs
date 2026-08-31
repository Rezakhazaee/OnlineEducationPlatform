using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    // =========================
    // GET: api/Courses
    // دریافت لیست دوره‌ها
    // =========================

    [HttpGet]
    [Authorize]
    public async Task<List<CourseDto>> Get()
    {
        return await _context.Courses
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                InstructorId = c.InstructorId,
                IsActive = c.IsActive
            })
            .ToListAsync();
    }


    // =========================
    // POST: api/Courses
    // ایجاد دوره جدید
    // فقط Admin
    // =========================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CourseDto>> Create(CreateCourseDto dto)
    {
        var course = new Course
        {
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            InstructorId = dto.InstructorId,
            IsActive = dto.IsActive
        };

        _context.Courses.Add(course);

        await _context.SaveChangesAsync();

        var result = new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            InstructorId = course.InstructorId,
            IsActive = course.IsActive
        };

        return Ok(result);
    }
}