using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using BackEnd.Services;
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
    private readonly PackageAccessService _packageAccess;

    public LessonProgressController(
        ApplicationDbContext context,
        PackageAccessService packageAccess)
    {
        _context = context;
        _packageAccess = packageAccess;
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

    private async Task<bool> CanAccessEnrollmentAsync(
        Enrollment enrollment)
    {
        if (enrollment.Status != "Active")
        {
            return false;
        }

        if (!await _packageAccess.HasPackageAsync(2))
        {
            return false;
        }

        return enrollment.Course != null;
    }

    private async Task<bool> CanAccessLessonAsync(
        Enrollment enrollment,
        CourseLesson lesson)
    {
        if (!await CanAccessEnrollmentAsync(enrollment))
        {
            return false;
        }

        if (!lesson.IsActive ||
            lesson.CourseModule == null ||
            !lesson.CourseModule.IsActive)
        {
            return false;
        }

        if (lesson.CourseModule.CourseId != enrollment.CourseId)
        {
            return false;
        }

        if (lesson.IsFreePreview)
        {
            return true;
        }

        var coursePrice =
            enrollment.CoursePartnerOrganization?.AgreedPrice
            ?? enrollment.Course!.Price;

        if (coursePrice <= 0)
        {
            return true;
        }

        var totalPaid =
            await _context.Payments
                .Where(p =>
                    p.EnrollmentId == enrollment.Id &&
                    p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount)
            ?? 0;

        var activeLessonIds = await _context.CourseLessons
            .Where(l =>
                l.CourseModule != null &&
                l.CourseModule.CourseId == enrollment.CourseId &&
                l.CourseModule.IsActive &&
                l.IsActive)
            .OrderBy(l => l.CourseModule!.SortOrder)
            .ThenBy(l => l.CourseModule!.Id)
            .ThenBy(l => l.SortOrder)
            .ThenBy(l => l.Id)
            .Select(l => l.Id)
            .ToListAsync();

        var lessonIndex = activeLessonIds.IndexOf(lesson.Id);

        if (lessonIndex < 0)
        {
            return false;
        }

        var paidRatio = totalPaid / coursePrice;

        if (paidRatio < 0)
        {
            paidRatio = 0;
        }

        if (paidRatio > 1)
        {
            paidRatio = 1;
        }

        var accessibleLessons =
            (int)Math.Floor(
                activeLessonIds.Count * paidRatio);

        return lessonIndex < accessibleLessons;
    }

    [Authorize(Roles = "Student")]
    [HttpGet("enrollment/{enrollmentId}")]
    public async Task<ActionResult<List<LessonProgressDto>>> GetByEnrollment(
        int enrollmentId)
    {
        if (!await _packageAccess.HasPackageAsync(2))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        "دسترسی به محتوای آنلاین در پکیج پایه فعال نیست."
                });
        }

        var student = await GetCurrentStudent();

        if (student == null)
        {
            return Unauthorized(new
            {
                message = "پروفایل دانشجویی پیدا نشد"
            });
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.CoursePartnerOrganization)
            .FirstOrDefaultAsync(e =>
                e.Id == enrollmentId &&
                e.StudentId == student.Id);

        if (enrollment == null)
        {
            return NotFound(new
            {
                message = "ثبت‌نام پیدا نشد"
            });
        }

        if (!await CanAccessEnrollmentAsync(enrollment))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message = "ثبت‌نام این دوره فعال نیست."
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
        if (!await _packageAccess.HasPackageAsync(2))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        "دسترسی به محتوای آنلاین در پکیج پایه فعال نیست."
                });
        }

        var student = await GetCurrentStudent();

        if (student == null)
        {
            return Unauthorized(new
            {
                message = "پروفایل دانشجویی پیدا نشد"
            });
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.CoursePartnerOrganization)
            .FirstOrDefaultAsync(e =>
                e.Id == dto.EnrollmentId &&
                e.StudentId == student.Id &&
                e.Status == "Active");

        if (enrollment == null)
        {
            return NotFound(new
            {
                message = "ثبت‌نام پیدا نشد"
            });
        }

        if (enrollment.Course == null)
        {
            return NotFound(new
            {
                message = "دوره پیدا نشد"
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

        if (!lesson.IsActive || !lesson.CourseModule.IsActive)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message = "این درس فعال نیست."
                });
        }

        if (!await CanAccessLessonAsync(enrollment, lesson))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        "این درس هنوز در محدوده دسترسی شما قرار نگرفته است."
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
