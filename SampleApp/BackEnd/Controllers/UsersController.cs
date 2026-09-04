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