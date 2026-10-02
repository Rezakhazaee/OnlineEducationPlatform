using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CoursePartnerOrganizationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoursePartnerOrganizationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<IEnumerable<CoursePartnerOrganization>>> GetByCourse(int courseId)
    {
        var exists = await _context.Courses.AnyAsync(c => c.Id == courseId);

        if (!exists)
            return NotFound(new { message = "دوره پیدا نشد." });

        var items = await _context.CoursePartnerOrganizations
            .Include(x => x.PartnerOrganization)
            .Where(x => x.CourseId == courseId)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<CoursePartnerOrganization>> Create(CreateCoursePartnerOrganizationDto request)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId);

        if (course == null)
            return BadRequest(new { message = "دوره پیدا نشد." });

        var organization = await _context.PartnerOrganizations
            .FirstOrDefaultAsync(o => o.Id == request.PartnerOrganizationId);

        if (organization == null)
            return BadRequest(new { message = "سازمان طرف قرارداد پیدا نشد." });

        if (!organization.IsActive)
            return BadRequest(new { message = "سازمان طرف قرارداد غیرفعال است." });

        if (request.EndDate.HasValue && request.StartDate.HasValue && request.EndDate < request.StartDate)
            return BadRequest(new { message = "تاریخ پایان قرارداد نمی‌تواند قبل از تاریخ شروع باشد." });

        var duplicate = await _context.CoursePartnerOrganizations.AnyAsync(x =>
            x.CourseId == request.CourseId &&
            x.PartnerOrganizationId == request.PartnerOrganizationId);

        if (duplicate)
            return Conflict(new { message = "این سازمان قبلاً برای این دوره ثبت شده است." });

        var item = new CoursePartnerOrganization
        {
            CourseId = request.CourseId,
            PartnerOrganizationId = request.PartnerOrganizationId,
            ContractNumber = request.ContractNumber?.Trim(),
            AgreedPrice = request.AgreedPrice,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            Description = request.Description?.Trim()
        };

        _context.CoursePartnerOrganizations.Add(item);
        await _context.SaveChangesAsync();

        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CoursePartnerOrganization>> Update(int id, UpdateCoursePartnerOrganizationDto request)
    {
        var item = await _context.CoursePartnerOrganizations
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound(new { message = "ارتباط دوره و سازمان پیدا نشد." });

        var organization = await _context.PartnerOrganizations
            .FirstOrDefaultAsync(o => o.Id == request.PartnerOrganizationId);

        if (organization == null)
            return BadRequest(new { message = "سازمان طرف قرارداد پیدا نشد." });

        if (!organization.IsActive)
            return BadRequest(new { message = "سازمان طرف قرارداد غیرفعال است." });

        if (request.EndDate.HasValue && request.StartDate.HasValue && request.EndDate < request.StartDate)
            return BadRequest(new { message = "تاریخ پایان قرارداد نمی‌تواند قبل از تاریخ شروع باشد." });

        var duplicate = await _context.CoursePartnerOrganizations.AnyAsync(x =>
            x.Id != id &&
            x.CourseId == item.CourseId &&
            x.PartnerOrganizationId == request.PartnerOrganizationId);

        if (duplicate)
            return Conflict(new { message = "این سازمان قبلاً برای این دوره ثبت شده است." });

        item.PartnerOrganizationId = request.PartnerOrganizationId;
        item.ContractNumber = request.ContractNumber?.Trim();
        item.AgreedPrice = request.AgreedPrice;
        item.StartDate = request.StartDate;
        item.EndDate = request.EndDate;
        item.IsActive = request.IsActive;
        item.Description = request.Description?.Trim();

        await _context.SaveChangesAsync();

        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.CoursePartnerOrganizations
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound(new { message = "ارتباط دوره و سازمان پیدا نشد." });

        var hasEnrollment = await _context.Enrollments
            .AnyAsync(e => e.CoursePartnerOrganizationId == id);

        if (hasEnrollment)
            return BadRequest(new
            {
                message = "این ارتباط دارای ثبت‌نام است و قابل حذف نیست. ابتدا آن را غیرفعال کنید."
            });

        _context.CoursePartnerOrganizations.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
