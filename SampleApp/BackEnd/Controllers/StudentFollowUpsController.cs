using System.Security.Claims;
using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentFollowUpsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentFollowUpsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Support - مشاهده پیگیری‌های دانشجویان خودش
    [Authorize(Roles = "Support")]
    [HttpGet("support/my")]
    public async Task<ActionResult<List<StudentFollowUp>>> GetMyFollowUps()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var supportUserId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        var followUps = await _context.StudentFollowUps
            .Where(f =>
                f.Student != null &&
                f.Student.SupportUserId == supportUserId)
            .Include(f => f.Student)
            .OrderByDescending(f => f.FollowUpDate)
            .Select(f => new
            {
                f.Id,
                f.StudentId,
                StudentName = f.Student != null
                    ? f.Student.FirstName + " " + f.Student.LastName
                    : string.Empty,
                f.SupportUserId,
                f.FollowUpDate,
                f.Status,
                f.Description,
                f.CreatedDate
            })
            .ToListAsync();

        return Ok(followUps);
    }

    // Support - ثبت پیگیری جدید
    [Authorize(Roles = "Support")]
    [HttpPost]
    public async Task<ActionResult> CreateFollowUp(
        CreateStudentFollowUpDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var supportUserId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        // بررسی اینکه دانشجو وجود دارد
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

        if (student == null)
        {
            return NotFound(new
            {
                message = "دانشجو پیدا نشد"
            });
        }

        // بررسی مالکیت دانشجو
        if (student.SupportUserId != supportUserId)
        {
            return Forbid();
        }

        // بررسی وضعیت پیگیری
        var allowedStatuses = new[]
        {
            "Pending",
            "InProgress",
            "Completed",
            "Cancelled"
        };

        if (!allowedStatuses.Contains(dto.Status))
        {
            return BadRequest(new
            {
                message = "وضعیت پیگیری معتبر نیست",
                allowedStatuses
            });
        }

        var followUp = new StudentFollowUp
        {
            StudentId = dto.StudentId,
            SupportUserId = supportUserId,
            FollowUpDate = dto.FollowUpDate,
            Status = dto.Status,
            Description = dto.Description,
            CreatedDate = DateTime.Now
        };

        _context.StudentFollowUps.Add(followUp);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "پیگیری با موفقیت ثبت شد",
            id = followUp.Id,
            studentId = followUp.StudentId,
            supportUserId = followUp.SupportUserId,
            followUpDate = followUp.FollowUpDate,
            status = followUp.Status,
            description = followUp.Description,
            createdDate = followUp.CreatedDate
        });
    }

    // Support - ویرایش پیگیری دانشجوی خودش
    [Authorize(Roles = "Support")]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateFollowUp(
        int id,
        UpdateStudentFollowUpDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var supportUserId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        // پیدا کردن پیگیری
        var followUp = await _context.StudentFollowUps
            .Include(f => f.Student)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (followUp == null)
        {
            return NotFound(new
            {
                message = "پیگیری پیدا نشد"
            });
        }

        // بررسی مالکیت دانشجو
        if (followUp.Student == null ||
            followUp.Student.SupportUserId != supportUserId)
        {
            return Forbid();
        }

        // وضعیت‌های مجاز
        var allowedStatuses = new[]
        {
            "Pending",
            "InProgress",
            "Completed",
            "Cancelled"
        };

        if (!allowedStatuses.Contains(dto.Status))
        {
            return BadRequest(new
            {
                message = "وضعیت پیگیری معتبر نیست",
                allowedStatuses
            });
        }

        // به‌روزرسانی اطلاعات پیگیری
        followUp.FollowUpDate = dto.FollowUpDate;
        followUp.Status = dto.Status;
        followUp.Description = dto.Description;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "پیگیری با موفقیت ویرایش شد",
            id = followUp.Id,
            studentId = followUp.StudentId,
            supportUserId = followUp.SupportUserId,
            followUpDate = followUp.FollowUpDate,
            status = followUp.Status,
            description = followUp.Description,
            createdDate = followUp.CreatedDate
        });
    }
}