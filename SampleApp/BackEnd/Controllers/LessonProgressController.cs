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

    private async Task<bool> CanAccessEnrollmentAsync(Enrollment enrollment)
    {
        if (enrollment.Status != "Active")
        {
            return false;
        }

        if (!await _packageAccess.HasPackageAsync(2))
        {
            return false;
        }

        if (enrollment.Course == null)
        {
            return false;
        }

        var coursePrice =
            enrollment.CoursePartnerOrganization?.AgreedPrice
            ?? enrollment.Course.Price;

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

        return totalPaid >= coursePrice;
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
                    message = "دسترسی به محتوای آنلاین در پکیج پایه فعال نیست."
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
                    message = "برای مشاهده پیشرفت دوره، وضعیت مالی ثبت‌نام باید تسویه شده باشد."
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
                    message = "دسترسی به محتوای آنلاین در پکیج پایه فعال نیست."
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
                e.Status != "Cancelled");

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
                    message = "برای ثبت پیشرفت درس، وضعیت مالی ثبت‌نام باید تسویه شده باشد."
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
