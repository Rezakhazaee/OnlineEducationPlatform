using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/StudentCourseContent")]
public class StudentCourseContentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PackageAccessService _packageAccess;

    public StudentCourseContentController(
        ApplicationDbContext context,
        PackageAccessService packageAccess)
    {
        _context = context;
        _packageAccess = packageAccess;
    }

    [Authorize(Roles = "Student")]
    [HttpGet("{courseId}")]
    public async Task<ActionResult<StudentCourseContentDto>> Get(
        int courseId)
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

        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
        {
            return NotFound(new
            {
                message = "پروفایل دانشجویی پیدا نشد"
            });
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.CoursePartnerOrganization)
            .FirstOrDefaultAsync(e =>
                e.StudentId == student.Id &&
                e.CourseId == courseId);

        if (enrollment == null)
        {
            return NotFound(new
            {
                message = "دسترسی به این دوره برای شما وجود ندارد"
            });
        }

        if (enrollment.Status != "Active")
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message = "ثبت‌نام این دوره فعال نیست."
                });
        }

        var course = enrollment.Course;

        if (course == null)
        {
            return NotFound(new
            {
                message = "دوره پیدا نشد"
            });
        }

        var coursePrice =
            enrollment.CoursePartnerOrganization?.AgreedPrice
            ?? course.Price;

        var totalPaid =
            await _context.Payments
                .Where(p =>
                    p.EnrollmentId == enrollment.Id &&
                    p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount)
            ?? 0;

        var modules = await _context.CourseModules
            .Where(m =>
                m.CourseId == courseId &&
                m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Id)
            .Select(m => new StudentCourseModuleDto
            {
                Id = m.Id,
                Title = m.Title,
                SortOrder = m.SortOrder,
                Lessons = m.Lessons
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.SortOrder)
                    .ThenBy(l => l.Id)
                    .Select(l => new StudentCourseLessonDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Description = l.Description,
                        ContentType = l.ContentType,
                        ContentUrl = l.ContentUrl,
                        DurationMinutes = l.DurationMinutes,
                        SortOrder = l.SortOrder,
                        IsFreePreview = l.IsFreePreview
                    })
                    .ToList()
            })
            .ToListAsync();

        var allLessons = modules
            .SelectMany(m => m.Lessons)
            .ToList();

        var totalLessons = allLessons.Count;
        var accessibleLessons = totalLessons;

        if (coursePrice > 0 && totalLessons > 0)
        {
            var paidRatio = totalPaid / coursePrice;

            if (paidRatio < 0)
            {
                paidRatio = 0;
            }

            if (paidRatio > 1)
            {
                paidRatio = 1;
            }

            accessibleLessons = (int)Math.Floor(
                totalLessons * paidRatio);
        }

        for (var i = 0; i < allLessons.Count; i++)
        {
            var lesson = allLessons[i];
            var isPaidAccess = i < accessibleLessons;

            lesson.IsLocked =
                !lesson.IsFreePreview && !isPaidAccess;

            if (lesson.IsLocked)
            {
                lesson.ContentUrl = null;
            }
        }

        return Ok(new StudentCourseContentDto
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            DeliveryType = course.DeliveryType,
            Modules = modules
        });
    }
}
