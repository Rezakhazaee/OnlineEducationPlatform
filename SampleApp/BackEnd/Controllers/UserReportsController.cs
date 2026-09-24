using BackEnd.Data;
using BackEnd.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UserReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<UserReportResultDto>> Get(
        [FromQuery] UserReportQueryDto query)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = Math.Clamp(query.PageSize, 5, 100);

        var users = _context.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            users = users.Where(u =>
                u.FullName.Contains(search) ||
                u.Mobile.Contains(search) ||
                u.Username.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
            users = users.Where(u => u.Role == query.Role);

        if (query.IsActive.HasValue)
            users = users.Where(
                u => u.IsActive == query.IsActive.Value);

        if (query.CreatedFrom.HasValue)
            users = users.Where(
                u => u.CreatedAt >= query.CreatedFrom.Value);

        if (query.CreatedTo.HasValue)
        {
            var endDate = query.CreatedTo.Value.Date.AddDays(1);

            users = users.Where(
                u => u.CreatedAt < endDate);
        }

        users = query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "fullname" => query.SortDescending
                ? users.OrderByDescending(u => u.FullName)
                : users.OrderBy(u => u.FullName),

            "mobile" => query.SortDescending
                ? users.OrderByDescending(u => u.Mobile)
                : users.OrderBy(u => u.Mobile),

            "username" => query.SortDescending
                ? users.OrderByDescending(u => u.Username)
                : users.OrderBy(u => u.Username),

            "role" => query.SortDescending
                ? users.OrderByDescending(u => u.Role)
                : users.OrderBy(u => u.Role),

            "status" => query.SortDescending
                ? users.OrderByDescending(u => u.IsActive)
                : users.OrderBy(u => u.IsActive),

            _ => query.SortDescending
                ? users.OrderByDescending(u => u.CreatedAt)
                : users.OrderBy(u => u.CreatedAt)
        };

        var totalCount = await users.CountAsync();

        var items = await users
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new UserReportItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Mobile = u.Mobile,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(new UserReportResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)query.PageSize)
        });
    }
}
