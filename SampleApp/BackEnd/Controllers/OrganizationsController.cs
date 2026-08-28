using BackEnd.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using BackEnd.Models;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrganizationsController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpGet]
public async Task<List<OrganizationDto>> Get()
{
    return await _context.Organizations
        .Select(o => new OrganizationDto
        {
            Id = o.Id,
            Name = o.Name,
            Description = o.Description,
            IsActive = o.IsActive
        })
        .ToListAsync();
}


    [HttpPost]
    public async Task<ActionResult<OrganizationDto>> Create(CreateOrganizationDto dto)
    {
        var organization = new Organization
{
    Name = dto.Name,
    Description = dto.Description,
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

return result;
    }
}