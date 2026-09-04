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
[Authorize(Roles = "Admin,Support")]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // دریافت لیست دانشجویان
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
        if (currentUser.Role == "Admin")
        {
            query = _context.Students;
        }
        // Support فقط دانشجویان اختصاص داده شده به خودش را می‌بیند
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
                OrganizationId = s.OrganizationId,
                MarketingUserId = s.MarketingUserId,
                SupportUserId = s.SupportUserId,
                CreatedDate = s.CreatedDate
            })
            .ToListAsync();

        return Ok(students);
    }

    // ثبت دانشجوی جدید
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
        if (currentUser.Role != "Admin")
        {
            return Forbid();
        }

        var student = new Student
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            NationalCode = dto.NationalCode,
            BirthDate = dto.BirthDate,
            Mobile = dto.Mobile,
            Address = dto.Address,
            GuardianName = dto.GuardianName,
            GuardianMobile = dto.GuardianMobile,
            OrganizationId = dto.OrganizationId,
            MarketingUserId = dto.MarketingUserId,
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
            OrganizationId = student.OrganizationId,
            MarketingUserId = student.MarketingUserId,
            SupportUserId = student.SupportUserId,
            CreatedDate = student.CreatedDate
        };

        return Ok(result);
    }

    // اختصاص دانشجو به کارشناس پشتیبانی
    [HttpPut("{studentId}/assign-support")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignSupport(
        int studentId,
        AssignSupportDto dto)
    {
        // پیدا کردن دانشجو
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
        {
            return NotFound(new
            {
                message = "دانشجو پیدا نشد"
            });
        }

        // پیدا کردن کارشناس پشتیبانی
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

        // اختصاص Support
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