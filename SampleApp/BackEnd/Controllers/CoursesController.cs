using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================
    // GET: api/Courses
    // دریافت لیست دوره‌ها
    // =========================

    [HttpGet]
    [Authorize]
    public async Task<List<CourseDto>> Get()
    {
        var query = _context.Courses.AsQueryable();

        // Instructor فقط دوره‌های خودش را می‌بیند
        if (User.IsInRole("Instructor"))
        {
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;

            if (!int.TryParse(userId, out var instructorId))
            {
                return new List<CourseDto>();
            }

            query = query.Where(c => c.InstructorId == instructorId);
        }

        return await query
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                InstructorId = c.InstructorId,
                IsActive = c.IsActive
            })
            .ToListAsync();
    }


    // =========================
    // GET: api/Courses/{id}
    // دریافت اطلاعات یک دوره
    // =========================

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<CourseDto>> GetById(int id)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id);

        // اگر دوره وجود نداشت
        if (course == null)
        {
            return NotFound(new
            {
                message = "دوره پیدا نشد"
            });
        }

        // Instructor فقط می‌تواند دوره خودش را مشاهده کند
        if (User.IsInRole("Instructor"))
        {
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;

            if (!int.TryParse(userId, out var instructorId))
            {
                return Unauthorized(new
                {
                    message = "شناسه کاربر معتبر نیست"
                });
            }

            // اگر دوره متعلق به این مدرس نیست
            if (course.InstructorId != instructorId)
            {
                return NotFound(new
                {
                    message = "دوره پیدا نشد"
                });
            }
        }

        var result = new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            InstructorId = course.InstructorId,
            IsActive = course.IsActive
        };

        return Ok(result);
    }


    // =========================
    // PUT: api/Courses/{id}
    // ویرایش دوره
    // =========================

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<CourseDto>> Update(
        int id,
        UpdateCourseDto dto)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id);

        // دوره پیدا نشد
        if (course == null)
        {
            return NotFound(new
            {
                message = "دوره پیدا نشد"
            });
        }

        // Instructor فقط می‌تواند دوره خودش را ویرایش کند
        if (User.IsInRole("Instructor"))
        {
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;

            if (!int.TryParse(userId, out var instructorId))
            {
                return Unauthorized(new
                {
                    message = "شناسه کاربر معتبر نیست"
                });
            }

            // دوره متعلق به این Instructor نیست
            if (course.InstructorId != instructorId)
            {
                return NotFound(new
                {
                    message = "دوره پیدا نشد"
                });
            }
        }

        // اطلاعات قابل ویرایش
        course.Title = dto.Title;
        course.Description = dto.Description;
        course.Price = dto.Price;
        course.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        var result = new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            InstructorId = course.InstructorId,
            IsActive = course.IsActive
        };

        return Ok(result);
    }


    // =========================
    // GET: api/Courses/instructors
    // دریافت لیست مدرس‌ها
    // فقط Admin
    // =========================

    [HttpGet("instructors")]
    [Authorize(Roles = "Admin")]
    public async Task<List<InstructorDto>> GetInstructors()
    {
        return await _context.Users
            .Where(u => u.Role == "Instructor")
            .Select(u => new InstructorDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Mobile = u.Mobile,
                Username = u.Username,
                IsActive = u.IsActive,

                CourseCount = _context.Courses
                    .Count(c => c.InstructorId == u.Id)
            })
            .ToListAsync();
    }


    // =========================
    // POST: api/Courses
    // ایجاد دوره
    // فقط Admin
    // =========================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CourseDto>> Create(
        CreateCourseDto dto)
    {
        // اگر برای دوره مدرس تعیین شده است
        if (dto.InstructorId.HasValue)
        {
            var instructor = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == dto.InstructorId.Value);

            // مدرس پیدا نشد
            if (instructor == null)
            {
                return BadRequest(new
                {
                    message = "مدرس موردنظر پیدا نشد"
                });
            }

            // کاربر نقش Instructor ندارد
            if (instructor.Role != "Instructor")
            {
                return BadRequest(new
                {
                    message = "کاربر انتخاب‌شده نقش Instructor ندارد"
                });
            }

            // مدرس غیرفعال است
            if (!instructor.IsActive)
            {
                return BadRequest(new
                {
                    message = "حساب مدرس غیرفعال است"
                });
            }
        }

        var course = new Course
        {
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            InstructorId = dto.InstructorId,
            IsActive = dto.IsActive
        };

        _context.Courses.Add(course);

        await _context.SaveChangesAsync();

        var result = new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            InstructorId = course.InstructorId,
            IsActive = course.IsActive
        };

        return Ok(result);
    }
}