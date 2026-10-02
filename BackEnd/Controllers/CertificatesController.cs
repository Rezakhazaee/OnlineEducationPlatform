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
[Authorize(Roles = "Student")]
public class CertificatesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CertificatesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("enrollment/{enrollmentId:int}/issue")]
    public async Task<ActionResult<CertificateDto>> Issue(int enrollmentId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e =>
                e.Id == enrollmentId &&
                e.Student != null &&
                e.Student.UserId == userId);

        if (enrollment == null || enrollment.Status == "Cancelled")
            return NotFound(new { message = "ثبت‌نام مورد نظر پیدا نشد." });

        var existing = await _context.Certificates
            .FirstOrDefaultAsync(c => c.EnrollmentId == enrollmentId);

        if (existing != null)
            return Ok(ToDto(existing));

        if (enrollment.Student == null || enrollment.Course == null)
            return BadRequest(new { message = "اطلاعات ثبت‌نام ناقص است." });

        var lessonIds = await _context.CourseLessons
            .Where(l =>
                l.IsActive &&
                l.CourseModule != null &&
                l.CourseModule.IsActive &&
                l.CourseModule.CourseId == enrollment.CourseId)
            .Select(l => l.Id)
            .ToListAsync();

        if (lessonIds.Count == 0)
        {
            return BadRequest(new
            {
                message = "این دوره هنوز درس فعالی ندارد."
            });
        }

        var completedCount = await _context.LessonProgresses
            .CountAsync(p =>
                p.EnrollmentId == enrollmentId &&
                p.IsCompleted &&
                lessonIds.Contains(p.CourseLessonId));

        if (completedCount != lessonIds.Count)
        {
            return BadRequest(new
            {
                message = "برای صدور گواهی باید تمام درس‌های دوره تکمیل شده باشند.",
                totalLessons = lessonIds.Count,
                completedLessons = completedCount,
                progressPercent = Math.Round(
                    completedCount * 100m / lessonIds.Count,
                    2)
            });
        }

        var certificate = new Certificate
        {
            EnrollmentId = enrollmentId,
            CertificateNumber =
                $"CERT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..28],
            StudentName =
                $"{enrollment.Student.FirstName} {enrollment.Student.LastName}".Trim(),
            CourseTitle = enrollment.Course.Title,
            IssuedAtUtc = DateTime.UtcNow
        };

        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync();

        return Ok(ToDto(certificate));
    }

    [HttpGet("enrollment/{enrollmentId:int}")]
    public async Task<ActionResult<CertificateDto>> GetByEnrollment(int enrollmentId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var certificate = await _context.Certificates
            .Include(c => c.Enrollment)
            .ThenInclude(e => e!.Student)
            .FirstOrDefaultAsync(c =>
                c.EnrollmentId == enrollmentId &&
                c.Enrollment != null &&
                c.Enrollment.Student != null &&
                c.Enrollment.Student.UserId == userId);

        if (certificate == null)
            return NotFound(new
            {
                message = "گواهی این ثبت‌نام صادر نشده است."
            });

        return Ok(ToDto(certificate));
    }

    private static CertificateDto ToDto(Certificate certificate)
    {
        return new CertificateDto
        {
            Id = certificate.Id,
            EnrollmentId = certificate.EnrollmentId,
            CertificateNumber = certificate.CertificateNumber,
            StudentName = certificate.StudentName,
            CourseTitle = certificate.CourseTitle,
            IssuedAtUtc = certificate.IssuedAtUtc
        };
    }
}
