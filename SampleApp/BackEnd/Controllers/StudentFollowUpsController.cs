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
[Authorize(Roles = "Admin,Support")]
public class StudentFollowUpsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentFollowUpsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Admin - مشاهده همه پیگیری‌ها
    // Support - مشاهده پیگیری‌های دانشجویان خودش
    [HttpGet]
    public async Task<ActionResult> GetFollowUps()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        var query = _context.StudentFollowUps
            .Include(f => f.Student)
            .AsQueryable();

        if (role == "Support")
        {
            query = query.Where(f =>
                f.Student != null &&
                f.Student.SupportUserId == userId);
        }

        var followUps = await query
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

    // Support - مشاهده پیگیری‌های دانشجویان خودش
    [Authorize(Roles = "Support")]
    [HttpGet("support/my")]
    public async Task<ActionResult> GetMyFollowUps()
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

    // Admin و Support - ثبت پیگیری
    [HttpPost]
    public async Task<ActionResult> CreateFollowUp(
        CreateStudentFollowUpDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

        if (student == null)
        {
            return NotFound(new
            {
                message = "دانشجو پیدا نشد"
            });
        }

        // Support فقط برای دانشجوی خودش اجازه ثبت دارد
        if (role == "Support" &&
            student.SupportUserId != userId)
        {
            return Forbid();
        }

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
            SupportUserId = role == "Support"
                ? userId
                : student.SupportUserId ?? userId,
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

    // Admin و Support - ویرایش پیگیری
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateFollowUp(
        int id,
        UpdateStudentFollowUpDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

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

        // Support فقط پیگیری دانشجوی خودش را ویرایش می‌کند
        if (role == "Support" &&
            (followUp.Student == null ||
             followUp.Student.SupportUserId != userId))
        {
            return Forbid();
        }

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

    // Admin و Support - حذف پیگیری
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFollowUp(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

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

        // Support فقط پیگیری دانشجوی خودش را حذف می‌کند
        if (role == "Support" &&
            (followUp.Student == null ||
             followUp.Student.SupportUserId != userId))
        {
            return Forbid();
        }

        _context.StudentFollowUps.Remove(followUp);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "پیگیری با موفقیت حذف شد",
            id = followUp.Id
        });
    }
}