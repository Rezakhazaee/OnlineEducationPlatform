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
public class PartnerOrganizationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PartnerOrganizationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var organizations = await _context.PartnerOrganizations
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.ContactPerson,
                x.ContactMobile,
                x.ContractNumber,
                x.ContractStartDate,
                x.ContractEndDate,
                x.IsActive,
                x.Description,
                x.CreatedDate,
                HasAccount = _context.Users.Any(u => u.PartnerOrganizationId == x.Id && u.Role == "PartnerOrganization"),
                AccountUsername = _context.Users.Where(u => u.PartnerOrganizationId == x.Id && u.Role == "PartnerOrganization").Select(u => u.Username).FirstOrDefault(),
                AccountIsActive = _context.Users.Where(u => u.PartnerOrganizationId == x.Id && u.Role == "PartnerOrganization").Select(u => (bool?)u.IsActive).FirstOrDefault()
            }).ToListAsync();
        return Ok(organizations);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var organization = await _context.PartnerOrganizations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.ContactPerson,
                x.ContactMobile,
                x.ContractNumber,
                x.ContractStartDate,
                x.ContractEndDate,
                x.IsActive,
                x.Description,
                x.CreatedDate,
                HasAccount = _context.Users.Any(u => u.PartnerOrganizationId == x.Id && u.Role == "PartnerOrganization"),
                AccountUsername = _context.Users.Where(u => u.PartnerOrganizationId == x.Id && u.Role == "PartnerOrganization").Select(u => u.Username).FirstOrDefault(),
                AccountIsActive = _context.Users.Where(u => u.PartnerOrganizationId == x.Id && u.Role == "PartnerOrganization").Select(u => (bool?)u.IsActive).FirstOrDefault()
            }).FirstOrDefaultAsync();
        if (organization == null)
            return NotFound(new { message = "سازمان همکار پیدا نشد." });
        return Ok(organization);
    }
    [HttpPost]
    public async Task<ActionResult<PartnerOrganization>> Create(
        [FromBody] CreatePartnerOrganizationDto request)
    {
        if (request.ContractEndDate.HasValue &&
            request.ContractStartDate.HasValue &&
            request.ContractEndDate < request.ContractStartDate)
        {
            return BadRequest(new
            {
                message = "تاریخ پایان قرارداد نمی‌تواند قبل از تاریخ شروع باشد."
            });
        }

        var organization = new PartnerOrganization
        {
            Name = request.Name.Trim(),
            ContactPerson = request.ContactPerson?.Trim(),
            ContactMobile = request.ContactMobile?.Trim(),
            ContractNumber = request.ContractNumber?.Trim(),
            ContractStartDate = request.ContractStartDate,
            ContractEndDate = request.ContractEndDate,
            IsActive = request.IsActive,
            Description = request.Description?.Trim(),
            CreatedDate = DateTime.Now
        };

        _context.PartnerOrganizations.Add(organization);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = organization.Id },
            organization);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PartnerOrganization>> Update(
        int id,
        [FromBody] UpdatePartnerOrganizationDto request)
    {
        var organization = await _context.PartnerOrganizations
            .FirstOrDefaultAsync(x => x.Id == id);

        if (organization == null)
            return NotFound(new { message = "سازمان طرف قرارداد پیدا نشد." });

        if (request.ContractEndDate.HasValue &&
            request.ContractStartDate.HasValue &&
            request.ContractEndDate < request.ContractStartDate)
        {
            return BadRequest(new
            {
                message = "تاریخ پایان قرارداد نمی‌تواند قبل از تاریخ شروع باشد."
            });
        }

        organization.Name = request.Name.Trim();
        organization.ContactPerson = request.ContactPerson?.Trim();
        organization.ContactMobile = request.ContactMobile?.Trim();
        organization.ContractNumber = request.ContractNumber?.Trim();
        organization.ContractStartDate = request.ContractStartDate;
        organization.ContractEndDate = request.ContractEndDate;
        organization.IsActive = request.IsActive;
        organization.Description = request.Description?.Trim();

        await _context.SaveChangesAsync();

        return Ok(organization);
    }

    [HttpPost("{id}/account")] public async Task<IActionResult> CreateAccount(int id, [FromBody] CreatePartnerOrganizationAccountDto request) { var organization = await _context.PartnerOrganizations.FirstOrDefaultAsync(x => x.Id == id); if (organization == null) return NotFound(new { message = "سازمان همکار پیدا نشد." }); if (!organization.IsActive) return BadRequest(new { message = "برای سازمان غیرفعال حساب ساخته نمی‌شود." }); if (await _context.Users.AnyAsync(u => u.PartnerOrganizationId == id)) return Conflict(new { message = "برای این سازمان قبلاً حساب ساخته شده است." }); var username = request.Username.Trim(); var mobile = request.Mobile.Trim(); if (await _context.Users.AnyAsync(u => u.Username == username)) return Conflict(new { message = "این نام کاربری قبلاً استفاده شده است." }); if (await _context.Users.AnyAsync(u => u.Mobile == mobile)) return Conflict(new { message = "این شماره موبایل قبلاً ثبت شده است." }); var user = new User { FullName = request.FullName.Trim(), Mobile = mobile, Username = username, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), Role = "PartnerOrganization", PartnerOrganizationId = id, IsActive = true, CreatedAt = DateTime.UtcNow }; _context.Users.Add(user); await _context.SaveChangesAsync(); return Ok(new { message = "حساب گزارش‌گیری سازمان با موفقیت ایجاد شد.", account = new { id = user.Id, fullName = user.FullName, mobile = user.Mobile, username = user.Username, role = user.Role, isActive = user.IsActive } }); }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var organization = await _context.PartnerOrganizations
            .FirstOrDefaultAsync(x => x.Id == id);

        if (organization == null)
            return NotFound(new { message = "سازمان طرف قرارداد پیدا نشد." });

        if (await _context.Users.AnyAsync(u => u.PartnerOrganizationId == id))
            return Conflict(new { message = "ابتدا حساب گزارش‌گیری سازمان باید تعیین تکلیف شود؛ حذف سازمان دارای حساب مجاز نیست." });
        _context.PartnerOrganizations.Remove(organization);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
