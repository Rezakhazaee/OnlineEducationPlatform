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
    // GET: api/PublicCourses
    // لیست عمومی دوره‌های فعال
    // =====================================================

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<PublicCourseListDto>>> GetCourses()
    {
        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.Id)
            .Select(c => new PublicCourseListDto
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

                ModuleCount = _context.CourseModules
                    .Count(m =>
                        m.CourseId == c.Id &&
                        m.IsActive),

                LessonCount = _context.CourseLessons
                    .Count(l =>
                        l.CourseModule.CourseId == c.Id &&
                        l.CourseModule.IsActive &&
                        l.IsActive)
            })
            .ToListAsync();

        return Ok(courses);
    }


    // =====================================================
    // GET: api/PublicCourses/{id}
    // جزئیات عمومی دوره
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
                                ContentUrl = l.IsFreePreview
                                    ? l.ContentUrl
                                    : null,
                                DurationMinutes = l.DurationMinutes,
                                SortOrder = l.SortOrder,
                                IsFreePreview = l.IsFreePreview
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
// DTO: لیست دوره‌ها
// =========================================================

public class PublicCourseListDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string DeliveryType { get; set; } = "Online";

    public string? InstructorName { get; set; }

    public int ModuleCount { get; set; }

    public int LessonCount { get; set; }
}


// =========================================================
// DTO: جزئیات دوره
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

    public int SortOrder { get; set; }

    public bool IsFreePreview { get; set; }
}