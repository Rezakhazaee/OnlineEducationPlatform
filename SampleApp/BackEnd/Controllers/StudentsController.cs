using BackEnd.DTOs;
using BackEnd.Data;
using BackEnd.Models;
using BackEnd.Services;
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
private readonly PackageAccessService _packageAccess;

    public StudentsController(
    ApplicationDbContext context,
    PackageAccessService packageAccess)
{
    _context = context;
    _packageAccess = packageAccess;
}

    // ==========================================
    // Student - مشاهده پروفایل خودش
    // ==========================================

    [Authorize(Roles = "Student")]
    [HttpGet("me")]
    public async Task<ActionResult<StudentDto>> GetMyProfile()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر در توکن پیدا نشد"
            });
        }

        if (!int.TryParse(
            userIdClaim.Value,
            out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s =>
                s.UserId == userId);

        if (student == null)
        {
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    message = "کاربر پیدا نشد"
                });
            }

            var nameParts = (currentUser.FullName ?? string.Empty)
                .Trim()
                .Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);

            var firstName = nameParts.Length > 0
                ? nameParts[0]
                : string.Empty;

            var lastName = nameParts.Length > 1
                ? nameParts[1]
                : string.Empty;

            student = new Student
            {
                UserId = userId,
                FirstName = firstName,
                LastName = lastName,
                NationalCode = string.Empty,
                Mobile = currentUser.Mobile,
                CreatedDate = DateTime.Now
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
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
            PartnerOrganizationId = student.PartnerOrganizationId,
            CreatedDate = student.CreatedDate
        };

        return Ok(result);
    }


    // ==========================================
    // دریافت لیست دانشجویان
    // ==========================================

    [Authorize(
        Roles = "Admin,EducationStaff,Marketer,Support,Instructor")]
    [HttpGet]
    public async Task<ActionResult<List<StudentDto>>> Get()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر در توکن پیدا نشد"
            });
        }

        if (!int.TryParse(
            userIdClaim.Value,
            out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (currentUser == null)
        {
            return Unauthorized(new
            {
                message = "کاربر پیدا نشد"
            });
        }

        IQueryable<Student> query =
            _context.Students;

        // Admin و EducationStaff
        // همه دانشجویان را می‌بینند.
        if (currentUser.Role == "Admin" ||
            currentUser.Role == "EducationStaff")
        {
            query = _context.Students;
        }

        // Support فقط دانشجویان اختصاص‌یافته به خودش
        else if (currentUser.Role == "Support")
        {
            query = _context.Students
                .Where(s =>
                    s.SupportUserId == currentUser.Id);
        }

        // Marketer فقط دانشجویانی که خودش معرف آنهاست
        else if (currentUser.Role == "Marketer")
        {
            query = _context.Students
                .Where(s =>
                    s.MarketingUserId == currentUser.Id);
        }

        // Instructor فقط دانشجویانی که در دوره‌های خودش ثبت‌نام کرده‌اند
        else if (currentUser.Role == "Instructor")
        {
            query = _context.Students
                .Where(s =>
                    _context.Enrollments.Any(e =>
                        e.StudentId == s.Id &&
                        e.InstructorId == currentUser.Id));
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
                PartnerOrganizationId = s.PartnerOrganizationId,
                CreatedDate = s.CreatedDate
            })
            .ToListAsync();

        return Ok(students);
    }


    // ==========================================
    // گزینه‌های لازم برای مدیریت دانشجویان
    // فقط Admin و EducationStaff
    // ==========================================

    [Authorize(Roles = "Admin,EducationStaff")]
    [HttpGet("management-options")]
    public async Task<
        ActionResult<List<StudentManagementOptionDto>>>
        GetManagementOptions()
    {
        var options = await _context.Users
            .Where(u =>
                u.IsActive &&
                (u.Role == "Marketer" ||
                 u.Role == "Support"))
            .OrderBy(u => u.Role)
            .ThenBy(u => u.FullName)
            .Select(u => new StudentManagementOptionDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Role = u.Role
            })
            .ToListAsync();

        return Ok(options);
    }


    // ==========================================
    // ثبت دانشجوی جدید
    // ==========================================

   // ==========================================
// ثبت دانشجوی جدید
// ==========================================

[Authorize(Roles = "Admin,EducationStaff")]
[HttpPost]
public async Task<ActionResult<StudentDto>> Create(
    CreateStudentDto dto)
{
    // =========================
    // بررسی کاربر جاری
    // =========================

    var userIdClaim =
        User.FindFirst(ClaimTypes.NameIdentifier);

    if (userIdClaim == null)
    {
        return Unauthorized(new
        {
            message = "شناسه کاربر در توکن پیدا نشد"
        });
    }

    if (!int.TryParse(
        userIdClaim.Value,
        out var userId))
    {
        return Unauthorized(new
        {
            message = "شناسه کاربر معتبر نیست"
        });
    }

    var currentUser = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (currentUser == null)
    {
        return Unauthorized(new
        {
            message = "کاربر پیدا نشد"
        });
    }

    if (currentUser.Role != "Admin" &&
        currentUser.Role != "EducationStaff")
    {
        return Forbid();
    }


    // =========================
    // بررسی بازاریاب
    // =========================

    if (dto.MarketingUserId.HasValue)
    {
        var marketer = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Id == dto.MarketingUserId.Value &&
                u.Role == "Marketer" &&
                u.IsActive);

        if (marketer == null)
        {
            return BadRequest(new
            {
                message = "بازاریاب انتخاب‌شده معتبر نیست."
            });
        }
    }


    // =========================
    // بررسی پشتیبان
    // =========================

    if (dto.SupportUserId.HasValue)
    {
        var support = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Id == dto.SupportUserId.Value &&
                u.Role == "Support" &&
                u.IsActive);

        if (support == null)
        {
            return BadRequest(new
            {
                message = "پشتیبان انتخاب‌شده معتبر نیست."
            });
        }
    }


    // =========================
    // بررسی سازمان طرف قرارداد
    // =========================

    if (dto.PartnerOrganizationId.HasValue)
    {
        // قابلیت سازمانی فقط در Package 4
        if (!await _packageAccess.HasPackageAsync(4))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        "اختصاص دانشجو به سازمان طرف قرارداد فقط در پکیج سازمانی فعال است."
                });
        }

        // پیدا کردن سازمان
        var partnerOrganization =
            await _context.PartnerOrganizations
                .FirstOrDefaultAsync(o =>
                    o.Id == dto.PartnerOrganizationId.Value);

        if (partnerOrganization == null)
        {
            return BadRequest(new
            {
                message = "سازمان طرف قرارداد پیدا نشد."
            });
        }

        // سازمان باید فعال باشد
        if (!partnerOrganization.IsActive)
        {
            return BadRequest(new
            {
                message = "سازمان طرف قرارداد غیرفعال است."
            });
        }
    }


    // =========================
    // ایجاد دانشجو
    // =========================

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

        MarketingUserId = dto.MarketingUserId,
        SupportUserId = dto.SupportUserId,

        // سازمان طرف قرارداد دانشجو
        PartnerOrganizationId = dto.PartnerOrganizationId,

        CreatedByUserId = currentUser.Id
    };


    _context.Students.Add(student);

    await _context.SaveChangesAsync();


    // =========================
    // نتیجه
    // =========================

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

        // برگرداندن سازمان دانشجو
        PartnerOrganizationId =
            student.PartnerOrganizationId,

        CreatedDate = student.CreatedDate
    };

    return Ok(result);
}
    // ==========================================
    // اختصاص دانشجو به Support
    // ==========================================

    [Authorize(Roles = "Admin,EducationStaff")]
    [HttpPut("{studentId}/assign-support")]
    public async Task<IActionResult> AssignSupport(
        int studentId,
        AssignSupportDto dto)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(
                s => s.Id == studentId);

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
                message =
                    "کارشناس پشتیبانی معتبر پیدا نشد"
            });
        }

        student.SupportUserId =
            supportUser.Id;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message =
                "دانشجو با موفقیت به کارشناس پشتیبانی اختصاص داده شد",

            studentId = student.Id,

            supportUserId = supportUser.Id,

            supportUserName =
                supportUser.FullName
        });
    }


    // ==========================================
    // Student - ویرایش پروفایل خودش
    // ==========================================

    [Authorize(Roles = "Student")]
    [HttpPut("me")]
    public async Task<ActionResult<StudentDto>> UpdateMyProfile(
        UpdateMyStudentProfileDto dto)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر در توکن پیدا نشد"
            });
        }

        if (!int.TryParse(
            userIdClaim.Value,
            out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s =>
                s.UserId == userId);

        if (student == null)
        {
            return NotFound(new
            {
                message = "پروفایل دانشجویی برای این کاربر پیدا نشد"
            });
        }

        var normalizedNationalCode =
            dto.NationalCode.Trim();

        var duplicateNationalCode =
            await _context.Students.AnyAsync(s =>
                s.Id != student.Id &&
                s.NationalCode == normalizedNationalCode);

        if (duplicateNationalCode)
        {
            return BadRequest(new
            {
                message = "این کد ملی قبلاً برای دانشجوی دیگری ثبت شده است."
            });
        }

        student.FirstName =
            dto.FirstName.Trim();

        student.LastName =
            dto.LastName.Trim();

        student.NationalCode =
            normalizedNationalCode;

        student.BirthDate =
            dto.BirthDate;

        student.Mobile =
            dto.Mobile.Trim();

        student.Address =
            string.IsNullOrWhiteSpace(dto.Address)
                ? null
                : dto.Address.Trim();

        student.GuardianName =
            string.IsNullOrWhiteSpace(dto.GuardianName)
                ? null
                : dto.GuardianName.Trim();

        student.GuardianMobile =
            string.IsNullOrWhiteSpace(dto.GuardianMobile)
                ? null
                : dto.GuardianMobile.Trim();

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

}
