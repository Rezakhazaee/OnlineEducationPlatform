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
[Route("api/[controller]")]
public class CourseLessonsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PackageAccessService _packageAccess;
public CourseLessonsController(
        ApplicationDbContext context,
        PackageAccessService packageAccess)
    {
        _context = context;
        _packageAccess = packageAccess;
    }

    private bool IsManagementUser =>
        User.IsInRole("Admin") ||
        User.IsInRole("EducationStaff");

    private async Task<int?> GetCurrentUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(value, out var userId)
            ? userId
            : null;
    }

    private async Task<bool> CanManageCourse(int courseId)
    {
        if (!await _packageAccess.HasPackageAsync(2))
        {
            return false;
        }

        if (IsManagementUser)
        {
            return true;
        }

        if (!User.IsInRole("Instructor"))
        {
            return false;
        }

        var userId = await GetCurrentUserId();

        return userId.HasValue &&
               await _context.Courses.AnyAsync(c =>
                   c.Id == courseId &&
                   c.InstructorId == userId.Value);
    }

    private async Task<CourseModule?> GetModule(int moduleId)
    {
        return await _context.CourseModules
            .FirstOrDefaultAsync(m => m.Id == moduleId);
    }

    [Authorize(Roles = "Admin,EducationStaff,Instructor")]
    [HttpGet("module/{moduleId}")]
    public async Task<ActionResult<List<CourseLessonDto>>> GetByModule(
        int moduleId)
    {
        var module = await GetModule(moduleId);

        if (module == null)
        {
            return NotFound(new
            {
                message = "سرفصل پیدا نشد"
            });
        }

        if (!await CanManageCourse(module.CourseId))
        {
            return NotFound(new
            {
                message = "سرفصل پیدا نشد"
            });
        }

        var lessons = await _context.CourseLessons
            .Where(l => l.CourseModuleId == moduleId)
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Id)
            .Select(l => new CourseLessonDto
            {
                Id = l.Id,
                CourseModuleId = l.CourseModuleId,
                Title = l.Title,
                Description = l.Description,
                ContentType = l.ContentType,
                ContentUrl = l.ContentUrl,
                DurationMinutes = l.DurationMinutes,
                SortOrder = l.SortOrder,
                IsActive = l.IsActive,
                IsFreePreview = l.IsFreePreview
            })
            .ToListAsync();

        return Ok(lessons);
    }

    [Authorize(Roles = "Admin,EducationStaff,Instructor")]
    [HttpPost]
    public async Task<ActionResult<CourseLessonDto>> Create(
        CreateCourseLessonDto dto)
    {
        var module = await GetModule(dto.CourseModuleId);

        if (module == null)
        {
            return NotFound(new
            {
                message = "سرفصل پیدا نشد"
            });
        }

        if (!await CanManageCourse(module.CourseId))
        {
            return NotFound(new
            {
                message = "سرفصل پیدا نشد"
            });
        }

        var lesson = new CourseLesson
        {
            CourseModuleId = dto.CourseModuleId,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            ContentType = dto.ContentType,
            ContentUrl = dto.ContentUrl,
            DurationMinutes = dto.DurationMinutes,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            IsFreePreview = dto.IsFreePreview
        };

        _context.CourseLessons.Add(lesson);

        await _context.SaveChangesAsync();

        return Ok(new CourseLessonDto
        {
            Id = lesson.Id,
            CourseModuleId = lesson.CourseModuleId,
            Title = lesson.Title,
            Description = lesson.Description,
            ContentType = lesson.ContentType,
            ContentUrl = lesson.ContentUrl,
            DurationMinutes = lesson.DurationMinutes,
            SortOrder = lesson.SortOrder,
            IsActive = lesson.IsActive,
            IsFreePreview = lesson.IsFreePreview
        });
    }

    [Authorize(Roles = "Admin,EducationStaff,Instructor")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CourseLessonDto>> Update(
        int id,
        UpdateCourseLessonDto dto)
    {
        var lesson = await _context.CourseLessons
            .Include(l => l.CourseModule)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null || lesson.CourseModule == null)
        {
            return NotFound(new
            {
                message = "درس پیدا نشد"
            });
        }

        if (!await CanManageCourse(lesson.CourseModule.CourseId))
        {
            return NotFound(new
            {
                message = "درس پیدا نشد"
            });
        }

        lesson.Title = dto.Title.Trim();
        lesson.Description = dto.Description;
        lesson.ContentType = dto.ContentType;
        lesson.ContentUrl = dto.ContentUrl;
        lesson.DurationMinutes = dto.DurationMinutes;
        lesson.SortOrder = dto.SortOrder;
        lesson.IsActive = dto.IsActive;
        lesson.IsFreePreview = dto.IsFreePreview;

        await _context.SaveChangesAsync();

        return Ok(new CourseLessonDto
        {
            Id = lesson.Id,
            CourseModuleId = lesson.CourseModuleId,
            Title = lesson.Title,
            Description = lesson.Description,
            ContentType = lesson.ContentType,
            ContentUrl = lesson.ContentUrl,
            DurationMinutes = lesson.DurationMinutes,
            SortOrder = lesson.SortOrder,
            IsActive = lesson.IsActive,
            IsFreePreview = lesson.IsFreePreview
        });
    }

    [Authorize(Roles = "Admin,EducationStaff,Instructor")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var lesson = await _context.CourseLessons
            .Include(l => l.CourseModule)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null || lesson.CourseModule == null)
        {
            return NotFound(new
            {
                message = "درس پیدا نشد"
            });
        }

        if (!await CanManageCourse(lesson.CourseModule.CourseId))
        {
            return NotFound(new
            {
                message = "درس پیدا نشد"
            });
        }

        _context.CourseLessons.Remove(lesson);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
