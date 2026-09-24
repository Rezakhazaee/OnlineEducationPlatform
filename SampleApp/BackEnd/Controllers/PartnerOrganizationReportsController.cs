using BackEnd.Data;
using BackEnd.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,EducationStaff")]
public class PartnerOrganizationReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PartnerOrganizationReportsController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PartnerOrganizationReportResultDto>> Get(
        [FromQuery] PartnerOrganizationReportQueryDto query)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = Math.Clamp(query.PageSize, 5, 100);

        var organizations = _context.PartnerOrganizations
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            organizations = organizations.Where(o =>
                o.Name.Contains(search) ||
                (o.ContactPerson != null &&
                 o.ContactPerson.Contains(search)) ||
                (o.ContactMobile != null &&
                 o.ContactMobile.Contains(search)) ||
                (o.ContractNumber != null &&
                 o.ContractNumber.Contains(search)));
        }

        if (query.IsActive.HasValue)
        {
            organizations = organizations.Where(
                o => o.IsActive == query.IsActive.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            organizations = organizations.Where(
                o => o.CreatedDate >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            var endDate = query.CreatedTo.Value.Date.AddDays(1);

            organizations = organizations.Where(
                o => o.CreatedDate < endDate);
        }

        if (query.ContractStartFrom.HasValue)
        {
            organizations = organizations.Where(o =>
                o.ContractStartDate.HasValue &&
                o.ContractStartDate.Value >=
                    query.ContractStartFrom.Value);
        }

        if (query.ContractStartTo.HasValue)
        {
            organizations = organizations.Where(o =>
                o.ContractStartDate.HasValue &&
                o.ContractStartDate.Value <=
                    query.ContractStartTo.Value);
        }

        if (query.ContractEndFrom.HasValue)
        {
            organizations = organizations.Where(o =>
                o.ContractEndDate.HasValue &&
                o.ContractEndDate.Value >=
                    query.ContractEndFrom.Value);
        }

        if (query.ContractEndTo.HasValue)
        {
            organizations = organizations.Where(o =>
                o.ContractEndDate.HasValue &&
                o.ContractEndDate.Value <=
                    query.ContractEndTo.Value);
        }

        organizations = query.SortBy?.Trim().ToLowerInvariant()
            switch
        {
            "name" => query.SortDescending
                ? organizations.OrderByDescending(o => o.Name)
                : organizations.OrderBy(o => o.Name),

            "contractstart" => query.SortDescending
                ? organizations.OrderByDescending(o => o.ContractStartDate)
                : organizations.OrderBy(o => o.ContractStartDate),

            "contractend" => query.SortDescending
                ? organizations.OrderByDescending(o => o.ContractEndDate)
                : organizations.OrderBy(o => o.ContractEndDate),

            "status" => query.SortDescending
                ? organizations.OrderByDescending(o => o.IsActive)
                : organizations.OrderBy(o => o.IsActive),

            _ => query.SortDescending
                ? organizations.OrderByDescending(o => o.CreatedDate)
                : organizations.OrderBy(o => o.CreatedDate)
        };

        var totalCount = await organizations.CountAsync();

        var pageItems = await organizations
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(o => new PartnerOrganizationReportItemDto
            {
                Id = o.Id,
                Name = o.Name,
                ContactPerson = o.ContactPerson,
                ContactMobile = o.ContactMobile,
                ContractNumber = o.ContractNumber,
                ContractStartDate = o.ContractStartDate,
                ContractEndDate = o.ContractEndDate,
                IsActive = o.IsActive,
                CreatedDate = o.CreatedDate,

                CourseCount = _context.CoursePartnerOrganizations
                    .Count(x =>
                        x.PartnerOrganizationId == o.Id),

                StudentCount = _context.Students
                    .Count(x =>
                        x.PartnerOrganizationId == o.Id)
            })
            .ToListAsync();

        foreach (var item in pageItems)
        {
            item.ContractStatus =
                GetContractStatus(
                    item.ContractStartDate,
                    item.ContractEndDate,
                    item.IsActive);
        }

        return Ok(new PartnerOrganizationReportResultDto
        {
            Items = pageItems,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)query.PageSize)
        });
    }

    private static string GetContractStatus(
        DateTime? start,
        DateTime? end,
        bool isActive)
    {
        if (!isActive)
            return "غیرفعال";

        var today = DateTime.Today;

        if (start.HasValue &&
            today < start.Value.Date)
        {
            return "آتی";
        }

        if (end.HasValue &&
            today > end.Value.Date)
        {
            return "منقضی";
        }

        return "فعال";
    }
}
