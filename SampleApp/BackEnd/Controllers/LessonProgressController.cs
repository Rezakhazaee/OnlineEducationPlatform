using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/LessonProgress")]
public class LessonProgressController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LessonProgressController(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<Student?> GetCurrentStudent()
    {
        var value =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(value, out var userId))
        {
            return null;
        }

        return await _context.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    [Authorize(Roles = "Student")]
    [HttpGet("enrollment/{enrollmentId}")]
    public async Task<ActionResult<List<LessonProgressDto>>> GetByEnrollment(
        int enrollmentId)
    {
        var student = await GetCurrentStudent();

        if (student == null)
        {
            return Unauthorized(new
            {
                message = "پروفایل دانشجویی پیدا نشد"
            });
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e =>
                e.Id == enrollmentId &&
                e.StudentId == student.Id &&
                e.Status != "Cancelled");

        if (enrollment == null)
        {
            return NotFound(new
            {
                message = "ثبت‌نام پیدا نشد"
            });
        }

        var progress = await _context.EnrollmentLessonProgresses
            .Where(p => p.EnrollmentId == enrollmentId)
            .OrderBy(p => p.CourseLessonId)
            .Select(p => new LessonProgressDto
            {
                CourseLessonId = p.CourseLessonId,
                IsCompleted = p.IsCompleted,
                CompletedAt = p.CompletedAt
            })
            .ToListAsync();

        return Ok(progress);
    }

    [Authorize(Roles = "Student")]
    [HttpPut]
    public async Task<ActionResult<LessonProgressDto>> Update(
        UpdateLessonProgressDto dto)
    {
        var student = await GetCurrentStudent();

        if (student == null)
        {
            return Unauthorized(new
            {
                message = "پروفایل دانشجویی پیدا نشد"
            });
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e =>
                e.Id == dto.EnrollmentId &&
                e.StudentId == student.Id &&
                e.Status != "Cancelled");

        if (enrollment == null)
        {
            return NotFound(new
            {
                message = "ثبت‌نام پیدا نشد"
            });
        }

        var lesson = await _context.CourseLessons
            .Include(l => l.CourseModule)
            .FirstOrDefaultAsync(l =>
                l.Id == dto.CourseLessonId);

        if (lesson == null || lesson.CourseModule == null)
        {
            return NotFound(new
            {
                message = "درس پیدا نشد"
            });
        }

        if (lesson.CourseModule.CourseId != enrollment.CourseId)
        {
            return BadRequest(new
            {
                message = "این درس متعلق به دوره ثبت‌نام‌شده نیست"
            });
        }

        var progress =
            await _context.EnrollmentLessonProgresses
                .FirstOrDefaultAsync(p =>
                    p.EnrollmentId == dto.EnrollmentId &&
                    p.CourseLessonId == dto.CourseLessonId);

        if (progress == null)
        {
            progress = new EnrollmentLessonProgress
            {
                EnrollmentId = dto.EnrollmentId,
                CourseLessonId = dto.CourseLessonId
            };

            _context.EnrollmentLessonProgresses.Add(progress);
        }

        progress.IsCompleted = dto.IsCompleted;

        progress.CompletedAt =
            dto.IsCompleted
                ? (progress.CompletedAt ?? DateTime.UtcNow)
                : null;

        await _context.SaveChangesAsync();

        return Ok(new LessonProgressDto
        {
            CourseLessonId = progress.CourseLessonId,
            IsCompleted = progress.IsCompleted,
            CompletedAt = progress.CompletedAt
        });
    }
}
