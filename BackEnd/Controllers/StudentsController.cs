using BackEnd.DTOs;
using BackEnd.Data;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }


    // Student - مشاهده پروفایل خودش
    [Authorize(Roles = "Student")]
    [HttpGet("me")]
    public async Task<ActionResult<StudentDto>> GetMyProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر در توکن پیدا نشد"
            });
        }

        if (!int.TryParse(userIdClaim.Value, out var userId))
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "کاربر پیدا نشد"
                });
            }

            var fullName = (user.FullName ?? string.Empty).Trim();
            var parts = fullName.Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);

            student = new Student
            {
                UserId = userId,
                FirstName = parts.Length > 0 ? parts[0] : "دانشجو",
                LastName = parts.Length > 1 ? parts[1] : string.Empty,
                Mobile = user.Mobile,
                CreatedDate = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }
        if (student == null)
        {
            return NotFound(new
            {
                message = "پروفایل دانشجویی برای این کاربر پیدا نشد"
            });
        }

        var result = new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            NationalCode = student.NationalCode,
            BirthDate = student.BirthDate,
            Mobile = student.Mobile,
            Address = student.Address,
            GuardianName = student.GuardianName,
            GuardianMobile = student.GuardianMobile,
            MarketingUserId = student.MarketingUserId,
            SupportUserId = student.SupportUserId,
            CreatedDate = student.CreatedDate
        };

        return Ok(result);
    }


    // دریافت لیست دانشجویان
    [Authorize(Roles = "Admin,EducationStaff,Marketer,Support")]
    [HttpGet]
    public async Task<ActionResult<List<StudentDto>>> Get()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر در توکن پیدا نشد"
            });
        }

        var userId = int.Parse(userIdClaim.Value);

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (currentUser == null)
        {
            return Unauthorized(new
            {
                message = "کاربر پیدا نشد"
            });
        }

        IQueryable<Student> query = _context.Students;

        // Admin می‌تواند همه دانشجویان را ببیند
        if (currentUser.Role == "Admin" || currentUser.Role == "EducationStaff")
        {
            query = _context.Students;
        }
        // Support فقط دانشجویان اختصاص داده شده به خودش را می‌بیند
        // Marketer فقط دانشجویان اختصاص داده شده به خودش را می‌بیند
        else if (currentUser.Role == "Marketer")
        {
            query = _context.Students
                .Where(s => s.MarketingUserId == currentUser.Id);
        }
        else if (currentUser.Role == "Support")
        {
            query = _context.Students
                .Where(s => s.SupportUserId == currentUser.Id);
        }
        else
        {
            return Forbid();
        }

        var students = await query
            .Select(s => new StudentDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                NationalCode = s.NationalCode,
                BirthDate = s.BirthDate,
                Mobile = s.Mobile,
                Address = s.Address,
                GuardianName = s.GuardianName,
                GuardianMobile = s.GuardianMobile,
                MarketingUserId = s.MarketingUserId,
                SupportUserId = s.SupportUserId,
                CreatedDate = s.CreatedDate
            })
            .ToListAsync();

        return Ok(students);
    }

    // حساب‌های Student که هنوز پروفایل دانشجویی ندارند
    [Authorize(Roles = "Admin,EducationStaff,Marketer")]
    [HttpGet("available-users")]
    public async Task<IActionResult> GetAvailableStudentUsers()
    {
        var users = await _context.Users
            .Where(u => u.Role == "Student" && u.IsActive)
            .Where(u => !_context.Students.Any(s => s.UserId == u.Id))
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Username,
                u.Mobile
            })
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return Ok(users);
    }

    // ثبت دانشجوی جدید
    [Authorize(Roles = "Admin,EducationStaff,Marketer")]
    [HttpPost]
    public async Task<ActionResult<StudentDto>> Create(CreateStudentDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر در توکن پیدا نشد"
            });
        }

        var userId = int.Parse(userIdClaim.Value);

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (currentUser == null)
        {
            return Unauthorized(new
            {
                message = "کاربر پیدا نشد"
            });
        }

        // فقط Admin می‌تواند دانشجوی جدید ایجاد کند
        if (currentUser.Role != "Admin" && currentUser.Role != "EducationStaff" && currentUser.Role != "Marketer")
        {
            return Forbid();
        }

        var student = new Student
        {
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            NationalCode = dto.NationalCode,
            BirthDate = dto.BirthDate,
            Mobile = dto.Mobile,
            Address = dto.Address,
            GuardianName = dto.GuardianName,
            GuardianMobile = dto.GuardianMobile,
            MarketingUserId = currentUser.Role == "Marketer" ? currentUser.Id : dto.MarketingUserId,
            SupportUserId = dto.SupportUserId
        };

        _context.Students.Add(student);

        await _context.SaveChangesAsync();

        var result = new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            NationalCode = student.NationalCode,
            BirthDate = student.BirthDate,
            Mobile = student.Mobile,
            Address = student.Address,
            GuardianName = student.GuardianName,
            GuardianMobile = student.GuardianMobile,
            MarketingUserId = student.MarketingUserId,
            SupportUserId = student.SupportUserId,
            CreatedDate = student.CreatedDate
        };

        return Ok(result);
    }
    // ویرایش اطلاعات دانشجو توسط Admin و EducationStaff
    [Authorize(Roles = "Admin,EducationStaff,Marketer,Support")]
    [HttpPut("{studentId}")]
    public async Task<ActionResult<StudentDto>> UpdateStudent(
        int studentId,
        UpdateMyStudentProfileRequest dto)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
        {
            return NotFound(new
            {
                message = "دانشجو پیدا نشد"
            });
        }
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if ((User.IsInRole("Marketer") && student.MarketingUserId != currentUserId) || (User.IsInRole("Support") && student.SupportUserId != currentUserId))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(dto.FirstName) ||
            string.IsNullOrWhiteSpace(dto.LastName) ||
            string.IsNullOrWhiteSpace(dto.NationalCode) ||
            string.IsNullOrWhiteSpace(dto.Mobile))
        {
            return BadRequest(new
            {
                message = "نام، نام خانوادگی، کد ملی و موبایل الزامی هستند."
            });
        }

        student.FirstName = dto.FirstName.Trim();
        student.LastName = dto.LastName.Trim();
        student.NationalCode = dto.NationalCode.Trim();
        student.BirthDate = dto.BirthDate;
        student.Mobile = dto.Mobile.Trim();
        student.Address = string.IsNullOrWhiteSpace(dto.Address)
            ? null
            : dto.Address.Trim();
        student.GuardianName = string.IsNullOrWhiteSpace(dto.GuardianName)
            ? null
            : dto.GuardianName.Trim();
        student.GuardianMobile = string.IsNullOrWhiteSpace(dto.GuardianMobile)
            ? null
            : dto.GuardianMobile.Trim();

        await _context.SaveChangesAsync();

        return Ok(new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            NationalCode = student.NationalCode,
            BirthDate = student.BirthDate,
            Mobile = student.Mobile,
            Address = student.Address,
            GuardianName = student.GuardianName,
            GuardianMobile = student.GuardianMobile,
            MarketingUserId = student.MarketingUserId,
            SupportUserId = student.SupportUserId,
            CreatedDate = student.CreatedDate
        });
    }

    // دریافت اطلاعات یک دانشجو برای Admin و EducationStaff
    [Authorize(Roles = "Admin,EducationStaff,Marketer,Support")]
    [HttpGet("{studentId:int}")]
    public async Task<ActionResult<StudentDto>> GetStudentById(int studentId)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
            return NotFound(new { message = "دانشجو پیدا نشد" });
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if ((User.IsInRole("Marketer") && student.MarketingUserId != currentUserId) || (User.IsInRole("Support") && student.SupportUserId != currentUserId))
        {
            return Forbid();
        }

        return Ok(new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            NationalCode = student.NationalCode,
            BirthDate = student.BirthDate,
            Mobile = student.Mobile,
            Address = student.Address,
            GuardianName = student.GuardianName,
            GuardianMobile = student.GuardianMobile,
            MarketingUserId = student.MarketingUserId,
            SupportUserId = student.SupportUserId,
            CreatedDate = student.CreatedDate
        });
    }
    public async Task<ActionResult<StudentDto>> UpdateMyProfile(
        UpdateMyStudentProfileRequest dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !int.TryParse(userIdClaim.Value, out var userId))
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

        if (string.IsNullOrWhiteSpace(dto.FirstName) ||
            string.IsNullOrWhiteSpace(dto.LastName) ||
            string.IsNullOrWhiteSpace(dto.NationalCode) ||
            string.IsNullOrWhiteSpace(dto.Mobile))
        {
            return BadRequest(new
            {
                message = "نام، نام خانوادگی، کد ملی و موبایل الزامی هستند."
            });
        }

        student.FirstName = dto.FirstName.Trim();
        student.LastName = dto.LastName.Trim();
        student.NationalCode = dto.NationalCode.Trim();
        student.BirthDate = dto.BirthDate;
        student.Mobile = dto.Mobile.Trim();
        student.Address = string.IsNullOrWhiteSpace(dto.Address)
            ? null
            : dto.Address.Trim();
        student.GuardianName = string.IsNullOrWhiteSpace(dto.GuardianName)
            ? null
            : dto.GuardianName.Trim();
        student.GuardianMobile = string.IsNullOrWhiteSpace(dto.GuardianMobile)
            ? null
            : dto.GuardianMobile.Trim();

        await _context.SaveChangesAsync();

        return Ok(new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            NationalCode = student.NationalCode,
            BirthDate = student.BirthDate,
            Mobile = student.Mobile,
            Address = student.Address,
            GuardianName = student.GuardianName,
            GuardianMobile = student.GuardianMobile,
            MarketingUserId = student.MarketingUserId,
            SupportUserId = student.SupportUserId,
            CreatedDate = student.CreatedDate
        });
    }

    // اختصاص دانشجو به کارشناس پشتیبانی
    [HttpPut("{studentId}/assign-support")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignSupport(
        int studentId,
        AssignSupportDto dto)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
        {
            return NotFound(new
            {
                message = "دانشجو پیدا نشد"
            });
        }

        var supportUser = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Id == dto.SupportUserId &&
                u.Role == "Support" &&
                u.IsActive);

        if (supportUser == null)
        {
            return BadRequest(new
            {
                message = "کارشناس پشتیبانی معتبر پیدا نشد"
            });
        }

        student.SupportUserId = supportUser.Id;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "دانشجو با موفقیت به کارشناس پشتیبانی اختصاص داده شد",
            studentId = student.Id,
            supportUserId = supportUser.Id,
            supportUserName = supportUser.FullName
        });
    }
}
