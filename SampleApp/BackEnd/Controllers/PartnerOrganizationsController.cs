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
    public async Task<ActionResult<IEnumerable<PartnerOrganization>>> GetAll()
    {
        var organizations = await _context.PartnerOrganizations
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(organizations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PartnerOrganization>> GetById(int id)
    {
        var organization = await _context.PartnerOrganizations
            .FirstOrDefaultAsync(x => x.Id == id);

        if (organization == null)
            return NotFound(new { message = "سازمان طرف قرارداد پیدا نشد." });

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var organization = await _context.PartnerOrganizations
            .FirstOrDefaultAsync(x => x.Id == id);

        if (organization == null)
            return NotFound(new { message = "سازمان طرف قرارداد پیدا نشد." });

        _context.PartnerOrganizations.Remove(organization);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
