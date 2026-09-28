using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BackEnd.Services;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseModulesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PackageAccessService _packageAccess;
    public CourseModulesController(
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

    private async Task<Course?> GetCourseForAccess(int courseId)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId);
    }

private async Task<bool> CanManageCourse(int courseId)
{
    // مدیریت محتوای آنلاین از پکیج 2 به بالا فعال است
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

    [Authorize(Roles = "Admin,EducationStaff,Instructor")]
    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<List<CourseModuleDto>>> GetByCourse(
        int courseId)
    {
        var course = await GetCourseForAccess(courseId);

        if (course == null)
        {
            return NotFound(new
            {
                message = "دوره پیدا نشد"
            });
        }

        if (!await CanManageCourse(courseId))
        {
            return NotFound(new
            {
                message = "دوره پیدا نشد"
            });
        }

        var modules = await _context.CourseModules
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Id)
            .Select(m => new CourseModuleDto
            {
                Id = m.Id,
                CourseId = m.CourseId,
                Title = m.Title,
                SortOrder = m.SortOrder,
                IsActive = m.IsActive,
                LessonCount = m.Lessons.Count
            })
            .ToListAsync();

        return Ok(modules);
    }

    [Authorize(Roles = "Admin,EducationStaff,Instructor")]
    [HttpPost]
    public async Task<ActionResult<CourseModuleDto>> Create(
        CreateCourseModuleDto dto)
    {
        if (!await CanManageCourse(dto.CourseId))
        {
            return NotFound(new
            {
                message = "دوره پیدا نشد"
            });
        }

        var module = new CourseModule
        {
            CourseId = dto.CourseId,
            Title = dto.Title.Trim(),
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive
        };

        _context.CourseModules.Add(module);

        await _context.SaveChangesAsync();

        return Ok(new CourseModuleDto
        {
            Id = module.Id,
            CourseId = module.CourseId,
            Title = module.Title,
            SortOrder = module.SortOrder,
            IsActive = module.IsActive,
            LessonCount = 0
        });
    }

    [Authorize(Roles = "Admin,EducationStaff,Instructor")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CourseModuleDto>> Update(
        int id,
        UpdateCourseModuleDto dto)
    {
        var module = await _context.CourseModules
            .FirstOrDefaultAsync(m => m.Id == id);

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

        module.Title = dto.Title.Trim();
        module.SortOrder = dto.SortOrder;
        module.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        var lessonCount = await _context.CourseLessons
            .CountAsync(l => l.CourseModuleId == module.Id);

        return Ok(new CourseModuleDto
        {
            Id = module.Id,
            CourseId = module.CourseId,
            Title = module.Title,
            SortOrder = module.SortOrder,
            IsActive = module.IsActive,
            LessonCount = lessonCount
        });
    }

    [Authorize(Roles = "Admin,EducationStaff,Instructor")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var module = await _context.CourseModules
            .FirstOrDefaultAsync(m => m.Id == id);

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

        _context.CourseModules.Remove(module);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
