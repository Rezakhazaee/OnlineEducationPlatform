using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationThemesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrganizationThemesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================
    // GET: api/OrganizationThemes/{organizationId}
    // Public - بدون نیاز به Login
    // =========================

    [HttpGet("{organizationId}")]
    [AllowAnonymous]
    public async Task<ActionResult<OrganizationTheme>> GetTheme(int organizationId)
    {
        var organization = await _context.Organizations
            .FindAsync(organizationId);

        if (organization == null)
            return NotFound("Organization not found.");

        var theme = await _context.OrganizationThemes
            .FirstOrDefaultAsync(t => t.OrganizationId == organizationId);

        if (theme == null)
        {
            theme = new OrganizationTheme
            {
                OrganizationId = organizationId,
                PrimaryColor = "#568fa8",
                SecondaryColor = "#263d4a",
                IsActive = true
            };

            _context.OrganizationThemes.Add(theme);
            await _context.SaveChangesAsync();
        }

        return Ok(theme);
    }

    // =========================
    // PUT: api/OrganizationThemes/{organizationId}
    // فقط Admin
    // =========================

    [HttpPut("{organizationId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrganizationTheme>> UpdateTheme(
        int organizationId,
        [FromBody] UpdateOrganizationThemeDto request)
    {
        var organization = await _context.Organizations
            .FindAsync(organizationId);

        if (organization == null)
            return NotFound("Organization not found.");

        var theme = await _context.OrganizationThemes
            .FirstOrDefaultAsync(t => t.OrganizationId == organizationId);

        if (theme == null)
        {
            theme = new OrganizationTheme
            {
                OrganizationId = organizationId
            };

            _context.OrganizationThemes.Add(theme);
        }

        theme.PrimaryColor = request.PrimaryColor;
        theme.SecondaryColor = request.SecondaryColor;
        theme.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return Ok(theme);
    }
}