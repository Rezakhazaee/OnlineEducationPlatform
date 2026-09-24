using BackEnd.Data;
using BackEnd.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,EducationStaff")]
public class CourseReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CourseReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<CourseReportResultDto>> Get(
        [FromQuery] CourseReportQueryDto query)
    {
        var courses = _context.Courses
            .AsNoTracking()
            .Include(c => c.Instructor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            courses = courses.Where(c =>
                c.Title.Contains(search) ||
                (c.Description != null && c.Description.Contains(search)));
        }

        if (query.InstructorId.HasValue)
        {
            courses = courses.Where(
                c => c.InstructorId == query.InstructorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.DeliveryType))
        {
            courses = courses.Where(
                c => c.DeliveryType == query.DeliveryType);
        }

        if (query.IsActive.HasValue)
        {
            courses = courses.Where(
                c => c.IsActive == query.IsActive.Value);
        }

        if (query.MinPrice.HasValue)
        {
            courses = courses.Where(
                c => c.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            courses = courses.Where(
                c => c.Price <= query.MaxPrice.Value);
        }

        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = Math.Clamp(query.PageSize, 5, 100);

        courses = query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "price" => query.SortDescending
                ? courses.OrderByDescending(c => c.Price)
                : courses.OrderBy(c => c.Price),

            "deliverytype" => query.SortDescending
                ? courses.OrderByDescending(c => c.DeliveryType)
                : courses.OrderBy(c => c.DeliveryType),

            "instructor" => query.SortDescending
                ? courses.OrderByDescending(c => c.Instructor!.FullName)
                : courses.OrderBy(c => c.Instructor!.FullName),

            "status" => query.SortDescending
                ? courses.OrderByDescending(c => c.IsActive)
                : courses.OrderBy(c => c.IsActive),

            _ => query.SortDescending
                ? courses.OrderByDescending(c => c.Title)
                : courses.OrderBy(c => c.Title)
        };

        var totalCount = await courses.CountAsync();

        var items = await courses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CourseReportItemDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                DeliveryType = c.DeliveryType,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor != null
                    ? c.Instructor.FullName
                    : null,
                IsActive = c.IsActive
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)query.PageSize);

        return Ok(new CourseReportResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        });
    }
}
