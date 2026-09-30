using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
public class LessonProgressController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LessonProgressController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("enrollment/{enrollmentId:int}")]
    public async Task<ActionResult<LessonProgressSummaryDto>> GetByEnrollment(
        int enrollmentId)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e =>
                e.Id == enrollmentId &&
                e.Student != null &&
                e.Student.UserId == userId);

        if (enrollment == null ||
            enrollment.Status == "Cancelled")
        {
            return NotFound(new
            {
                message = "ثبت‌نام مورد نظر پیدا نشد."
            });
        }

        var lessons = await _context.CourseLessons
            .Where(l =>
                l.IsActive &&
                l.CourseModule != null &&
                l.CourseModule.IsActive &&
                l.CourseModule.CourseId == enrollment.CourseId)
            .OrderBy(l => l.CourseModule!.SortOrder)
            .ThenBy(l => l.SortOrder)
            .ThenBy(l => l.Id)
            .Select(l => new
            {
                l.Id
            })
            .ToListAsync();

        var lessonIds = lessons
            .Select(x => x.Id)
            .ToList();

        var progress = await _context.LessonProgresses
            .Where(p =>
                p.EnrollmentId == enrollmentId &&
                lessonIds.Contains(p.CourseLessonId))
            .ToDictionaryAsync(
                p => p.CourseLessonId,
                p => p);

        var items = lessonIds
            .Select(lessonId =>
            {
                progress.TryGetValue(
                    lessonId,
                    out var item);

                return new LessonProgressItemDto
                {
                    LessonId = lessonId,
                    IsCompleted = item?.IsCompleted ?? false,
                    CompletedAt = item?.CompletedAt
                };
            })
            .ToList();

        var total = items.Count;
        var completed = items.Count(x => x.IsCompleted);

        return Ok(new LessonProgressSummaryDto
        {
            EnrollmentId = enrollmentId,
            CourseId = enrollment.CourseId,
            TotalLessons = total,
            CompletedLessons = completed,
            ProgressPercent = total == 0
                ? 0
                : Math.Round(
                    completed * 100m / total,
                    2),
            Lessons = items
        });
    }

    [HttpPut("enrollment/{enrollmentId:int}/lesson/{lessonId:int}")]
    public async Task<ActionResult<LessonProgressItemDto>> Update(
        int enrollmentId,
        int lessonId,
        UpdateLessonProgressDto dto)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e =>
                e.Id == enrollmentId &&
                e.Student != null &&
                e.Student.UserId == userId);

        if (enrollment == null ||
            enrollment.Status == "Cancelled")
        {
            return NotFound(new
            {
                message = "ثبت‌نام مورد نظر پیدا نشد."
            });
        }

        var lessonExists = await _context.CourseLessons
            .AnyAsync(l =>
                l.Id == lessonId &&
                l.IsActive &&
                l.CourseModule != null &&
                l.CourseModule.IsActive &&
                l.CourseModule.CourseId == enrollment.CourseId);

        if (!lessonExists)
        {
            return NotFound(new
            {
                message = "درس مورد نظر پیدا نشد."
            });
        }

        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(p =>
                p.EnrollmentId == enrollmentId &&
                p.CourseLessonId == lessonId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                EnrollmentId = enrollmentId,
                CourseLessonId = lessonId
            };

            _context.LessonProgresses.Add(progress);
        }

        progress.IsCompleted = dto.Completed;
        progress.CompletedAt =
            dto.Completed ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();

        return Ok(new LessonProgressItemDto
        {
            LessonId = lessonId,
            IsCompleted = progress.IsCompleted,
            CompletedAt = progress.CompletedAt
        });
    }
}
