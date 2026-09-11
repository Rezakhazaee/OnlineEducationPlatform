using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrganizationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // دریافت لیست سازمان‌ها
    [HttpGet]
    public async Task<ActionResult<List<OrganizationDto>>> Get()
    {
        var organizations = await _context.Organizations
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDto
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
                IsActive = o.IsActive
            })
            .ToListAsync();

        return Ok(organizations);
    }

    // دریافت یک سازمان
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrganizationDto>> GetById(int id)
    {
        var organization = await _context.Organizations
            .Where(o => o.Id == id)
            .Select(o => new OrganizationDto
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
                IsActive = o.IsActive
            })
            .FirstOrDefaultAsync();

        if (organization == null)
            return NotFound(new
            {
                message = "سازمان مورد نظر پیدا نشد."
            });

        return Ok(organization);
    }

    // ایجاد سازمان جدید
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrganizationDto>> Create(
        CreateOrganizationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "نام سازمان الزامی است."
            });
        }

        var name = dto.Name.Trim();

        var exists = await _context.Organizations
            .AnyAsync(o => o.Name == name);

        if (exists)
        {
            return Conflict(new
            {
                message = "سازمانی با این نام قبلاً ثبت شده است."
            });
        }

        var organization = new Organization
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim(),
            IsActive = dto.IsActive
        };

        _context.Organizations.Add(organization);

        await _context.SaveChangesAsync();

        var result = new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            IsActive = organization.IsActive
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = organization.Id },
            result);
    }

    // ویرایش سازمان
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrganizationDto>> Update(
        int id,
        UpdateOrganizationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "نام سازمان الزامی است."
            });
        }

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization == null)
        {
            return NotFound(new
            {
                message = "سازمان مورد نظر پیدا نشد."
            });
        }

        var name = dto.Name.Trim();

        var duplicateName = await _context.Organizations
            .AnyAsync(o =>
                o.Id != id &&
                o.Name == name);

        if (duplicateName)
        {
            return Conflict(new
            {
                message = "سازمان دیگری با این نام وجود دارد."
            });
        }

        organization.Name = name;
        organization.Description =
            string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();

        organization.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            IsActive = organization.IsActive
        });
    }

    // فعال / غیرفعال کردن سازمان
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrganizationDto>> UpdateStatus(
        int id,
        [FromBody] bool isActive)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization == null)
        {
            return NotFound(new
            {
                message = "سازمان مورد نظر پیدا نشد."
            });
        }

        organization.IsActive = isActive;

        await _context.SaveChangesAsync();

        return Ok(new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            IsActive = organization.IsActive
        });
    }
}
