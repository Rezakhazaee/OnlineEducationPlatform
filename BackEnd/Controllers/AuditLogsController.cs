using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BackEnd.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public AuditLogsController(ApplicationDbContext context) { _context = context; }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] int? userId = null, [FromQuery] string? action = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var query = _context.AuditLogs.AsNoTracking();
        if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(x => x.Action.Contains(action));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new { x.Id, x.UserId, x.Username, x.Role, x.Action, x.Entity,
                x.HttpMethod, x.Path, x.StatusCode, x.IpAddress, x.CreatedAt }).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }
}