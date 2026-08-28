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
    public async Task<List<Organization>> Get()
    {
        return await _context.Organizations.ToListAsync();
    }


    [HttpPost]
    public async Task<ActionResult<Organization>> Create(Organization organization)
    {
        _context.Organizations.Add(organization);

        await _context.SaveChangesAsync();

        return organization;
    }
}