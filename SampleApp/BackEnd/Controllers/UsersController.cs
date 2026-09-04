using BackEnd.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using BackEnd.Models;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // دریافت لیست کاربران
    [HttpGet]
    public async Task<List<UserDto>> Get()
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Mobile = u.Mobile,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    // دریافت اطلاعات یک کاربر
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "کاربر پیدا نشد"
            });
        }

        var result = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Mobile = user.Mobile,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(result);
    }

    // فعال / غیرفعال کردن کاربر
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateUserStatusDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "کاربر پیدا نشد"
            });
        }

        // جلوگیری از غیرفعال کردن Admin اصلی
        if (user.Id == 1 && !dto.IsActive)
        {
            return BadRequest(new
            {
                message = "Admin اصلی سیستم را نمی‌توان غیرفعال کرد"
            });
        }

        user.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = dto.IsActive
                ? "کاربر با موفقیت فعال شد"
                : "کاربر با موفقیت غیرفعال شد",
            userId = user.Id,
            username = user.Username,
            isActive = user.IsActive
        });
    }

    // تغییر Role کاربر توسط Admin
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(
        int id,
        UpdateUserRoleDto dto)
    {
        var allowedRoles = new[]
        {
            "Admin",
            "Instructor",
            "Support",
            "Student"
        };

        // بررسی Role
        if (!allowedRoles.Contains(dto.Role))
        {
            return BadRequest(new
            {
                message = "Role وارد شده معتبر نیست",
                allowedRoles = allowedRoles
            });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "کاربر پیدا نشد"
            });
        }

        // جلوگیری از تغییر Role Admin اصلی
        if (user.Id == 1)
        {
            return BadRequest(new
            {
                message = "Role مربوط به Admin اصلی سیستم قابل تغییر نیست"
            });
        }

        user.Role = dto.Role;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Role کاربر با موفقیت تغییر کرد",
            userId = user.Id,
            username = user.Username,
            role = user.Role
        });
    }

    // ویرایش اطلاعات کاربر توسط Admin
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateUserDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "کاربر پیدا نشد"
            });
        }

        // بررسی Username تکراری
        var usernameExists = await _context.Users
            .AnyAsync(u =>
                u.Username == dto.Username &&
                u.Id != id);

        if (usernameExists)
        {
            return BadRequest(new
            {
                message = "این نام کاربری قبلاً توسط کاربر دیگری ثبت شده است"
            });
        }

        // بررسی Mobile تکراری
        var mobileExists = await _context.Users
            .AnyAsync(u =>
                u.Mobile == dto.Mobile &&
                u.Id != id);

        if (mobileExists)
        {
            return BadRequest(new
            {
                message = "این شماره موبایل قبلاً توسط کاربر دیگری ثبت شده است"
            });
        }

        user.FullName = dto.FullName;
        user.Mobile = dto.Mobile;
        user.Username = dto.Username;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "اطلاعات کاربر با موفقیت ویرایش شد",
            userId = user.Id,
            fullName = user.FullName,
            mobile = user.Mobile,
            username = user.Username
        });
    }
    // تغییر رمز عبور کاربر توسط Admin
[HttpPut("{id}/password")]
public async Task<IActionResult> ChangePassword(
    int id,
    ChangeUserPasswordDto dto)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == id);

    if (user == null)
    {
        return NotFound(new
        {
            message = "کاربر پیدا نشد"
        });
    }

    // Hash کردن رمز عبور جدید
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(
        dto.NewPassword
    );

    user.PasswordHash = passwordHash;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "رمز عبور کاربر با موفقیت تغییر کرد",
        userId = user.Id,
        username = user.Username
    });
}
    // ثبت کاربر جدید توسط Admin
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        CreateAdminUserDto dto)
    {
        // بررسی Role
        var allowedRoles = new[]
        {
            "Admin",
            "Instructor",
            "Support",
            "Student"
        };

        if (!allowedRoles.Contains(dto.Role))
        {
            return BadRequest(new
            {
                message = "Role وارد شده معتبر نیست",
                allowedRoles = allowedRoles
            });
        }

        // بررسی نام کاربری
        var usernameExists = await _context.Users
            .AnyAsync(u => u.Username == dto.Username);

        if (usernameExists)
        {
            return BadRequest(new
            {
                message = "این نام کاربری قبلاً ثبت شده است"
            });
        }

        // بررسی موبایل
        var mobileExists = await _context.Users
            .AnyAsync(u => u.Mobile == dto.Mobile);

        if (mobileExists)
        {
            return BadRequest(new
            {
                message = "این شماره موبایل قبلاً ثبت شده است"
            });
        }

        // Hash کردن رمز عبور
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(
            dto.Password
        );

        var user = new User
        {
            FullName = dto.FullName,
            Mobile = dto.Mobile,
            Username = dto.Username,
            PasswordHash = passwordHash,
            Role = dto.Role,
            IsActive = true
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var result = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Mobile = user.Mobile,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(result);
    }
}