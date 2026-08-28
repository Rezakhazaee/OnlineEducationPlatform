using BackEnd.DTOs;
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
public async Task<List<EnrollmentDto>> Get()
{
    return await _context.Enrollments
        .Select(e => new EnrollmentDto
        {
            Id = e.Id,
            StudentId = e.StudentId,
            CourseId = e.CourseId,
            SupportUserId = e.SupportUserId,
            InstructorId = e.InstructorId,
            StartDate = e.StartDate,
            Status = e.Status
        })
        .ToListAsync();
}


    // POST: api/Enrollments
    [HttpPost]
    public async Task<ActionResult<EnrollmentDto>> Create(CreateEnrollmentDto dto)
    {
        var enrollment = new Enrollment
{
    StudentId = dto.StudentId,
    CourseId = dto.CourseId,
    SupportUserId = dto.SupportUserId,
    InstructorId = dto.InstructorId,
    StartDate = dto.StartDate,
    Status = dto.Status
};

_context.Enrollments.Add(enrollment);

await _context.SaveChangesAsync();


var result = new EnrollmentDto
{
    Id = enrollment.Id,
    StudentId = enrollment.StudentId,
    CourseId = enrollment.CourseId,
    SupportUserId = enrollment.SupportUserId,
    InstructorId = enrollment.InstructorId,
    StartDate = enrollment.StartDate,
    Status = enrollment.Status
};

return result;
    }
}