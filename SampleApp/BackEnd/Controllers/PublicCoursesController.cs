using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/PublicCourses")]
public class PublicCoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PublicCoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // GET: api/PublicCourses/{id}
    // اطلاعات عمومی یک دوره منتشرشده
    // =====================================================

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PublicCourseDto>> Get(int id)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Where(c =>
                c.Id == id &&
                c.IsActive)
            .Select(c => new PublicCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                DeliveryType = c.DeliveryType,

                InstructorName =
                    c.Instructor != null &&
                    c.Instructor.IsActive
                        ? c.Instructor.FullName
                        : null,

                Modules = _context.CourseModules
                    .Where(m =>
                        m.CourseId == c.Id &&
                        m.IsActive)
                    .OrderBy(m => m.SortOrder)
                    .ThenBy(m => m.Id)
                    .Select(m => new PublicCourseModuleDto
                    {
                        Id = m.Id,
                        Title = m.Title,
                        SortOrder = m.SortOrder,

                        Lessons = _context.CourseLessons
                            .Where(l =>
                                l.CourseModuleId == m.Id &&
                                l.IsActive)
                            .OrderBy(l => l.SortOrder)
                            .ThenBy(l => l.Id)
                            .Select(l => new PublicCourseLessonDto
                            {
                                Id = l.Id,
                                Title = l.Title,
                                Description = l.Description,
                                ContentType = l.ContentType,
                                DurationMinutes = l.DurationMinutes,
                                IsFreePreview = l.IsFreePreview,

                                // فقط محتوای دارای پیش‌نمایش عمومی
                                // برای بازدیدکننده قابل مشاهده باشد.
                                ContentUrl = l.IsFreePreview
                                    ? l.ContentUrl
                                    : null
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (course == null)
        {
            return NotFound(new
            {
                message = "دوره موردنظر پیدا نشد."
            });
        }

        return Ok(course);
    }
}


// =========================================================
// DTOs
// =========================================================

public class PublicCourseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string DeliveryType { get; set; } = "Online";

    public string? InstructorName { get; set; }

    public List<PublicCourseModuleDto> Modules { get; set; }
        = new();
}


public class PublicCourseModuleDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public List<PublicCourseLessonDto> Lessons { get; set; }
        = new();
}


public class PublicCourseLessonDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ContentType { get; set; } = "Video";

    public string? ContentUrl { get; set; }

    public int DurationMinutes { get; set; }

    public bool IsFreePreview { get; set; }
}