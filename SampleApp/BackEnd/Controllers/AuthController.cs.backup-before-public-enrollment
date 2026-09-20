using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // =========================
    // Register
    // =========================

    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserDto dto)
    {
        // بررسی نام کاربری تکراری
        var usernameExists = await _context.Users
            .AnyAsync(u => u.Username == dto.Username);

        if (usernameExists)
        {
            return BadRequest(new
            {
                message = "این نام کاربری قبلاً ثبت شده است"
            });
        }

        // بررسی شماره موبایل تکراری
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

        // ایجاد کاربر
        var user = new User
        {
            FullName = dto.FullName,
            Mobile = dto.Mobile,
            Username = dto.Username,
            PasswordHash = passwordHash,

            // نقش پیش‌فرض کاربران ثبت‌نام‌شده
            Role = "Student",

            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "ثبت نام با موفقیت انجام شد",
            userId = user.Id,
            username = user.Username,
            role = user.Role
        });
    }

    // =========================
    // Login
    // =========================

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        // پیدا کردن کاربر
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "نام کاربری یا رمز عبور اشتباه است"
            });
        }

        // بررسی فعال بودن حساب
        if (!user.IsActive)
        {
            return Unauthorized(new
            {
                message = "حساب کاربری شما غیرفعال است"
            });
        }

        // بررسی رمز عبور
        var passwordValid = BCrypt.Net.BCrypt.Verify(
            dto.Password,
            user.PasswordHash
        );

        if (!passwordValid)
        {
            return Unauthorized(new
            {
                message = "نام کاربری یا رمز عبور اشتباه است"
            });
        }

        // ساخت JWT Token
        var token = GenerateJwtToken(user);

        return Ok(new
        {
            message = "ورود با موفقیت انجام شد",
            token = token,
            user = new
            {
                id = user.Id,
                fullName = user.FullName,
                username = user.Username,
                role = user.Role
            }
        });
    }

    // =========================
    // Generate JWT Token
    // =========================

    private string GenerateJwtToken(User user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT Key تنظیم نشده است"
            );

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer تنظیم نشده است"
            );

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience تنظیم نشده است"
            );

        var expireMinutes = int.Parse(
            _configuration["Jwt:ExpireMinutes"] ?? "60"
        );

        // Claims
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),

            new Claim(
                ClaimTypes.Name,
                user.Username
            ),

            new Claim(
                ClaimTypes.Role,
                user.Role
            ),

            new Claim(
                "FullName",
                user.FullName
            )
        };

        // Security Key
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key)
        );

        // Credentials
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        // ایجاد Token
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                expireMinutes
            ),
            signingCredentials: credentials
        );

        // تبدیل Token به String
        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}