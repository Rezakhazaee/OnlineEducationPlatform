using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationSettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrganizationSettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================
    // GET: api/OrganizationSettings
    // دریافت تنظیمات سازمان
    // =========================

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<OrganizationSettings>> GetSettings()
    {
        var settings = await _context.OrganizationSettings
            .FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new OrganizationSettings
            {
                Name = "سازمان",
                Description = null,
                IsActive = true,
                PrimaryColor = "#568fa8",
                SecondaryColor = "#263d4a",
                ThemeIsActive = true
            };

            _context.OrganizationSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return Ok(settings);
    }

    // =========================
    // PUT: api/OrganizationSettings
    // فقط Admin
    // =========================

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrganizationSettings>> UpdateSettings(
        [FromBody] UpdateOrganizationSettingsDto request)
    {
        var settings = await _context.OrganizationSettings
            .FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new OrganizationSettings();

            _context.OrganizationSettings.Add(settings);
        }

        settings.Name = request.Name;
        settings.Description = request.Description;
        settings.IsActive = request.IsActive;
        settings.PrimaryColor = request.PrimaryColor;
        settings.SecondaryColor = request.SecondaryColor;
        settings.ThemeIsActive = request.ThemeIsActive;

        await _context.SaveChangesAsync();

        return Ok(settings);
    }
}
