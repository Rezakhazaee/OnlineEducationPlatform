using BackEnd.Data;
using BackEnd.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,EducationStaff")]
public class CoursePartnerOrganizationReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoursePartnerOrganizationReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<CoursePartnerOrganizationReportResultDto>> GetReport(
        [FromQuery] CoursePartnerOrganizationReportQueryDto query)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var itemsQuery = _context.CoursePartnerOrganizations
            .AsNoTracking()
            .Include(x => x.Course)
            .Include(x => x.PartnerOrganization)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            itemsQuery = itemsQuery.Where(x =>
                (x.Course != null && x.Course.Title.Contains(search)) ||
                (x.PartnerOrganization != null && x.PartnerOrganization.Name.Contains(search)) ||
                (x.ContractNumber != null && x.ContractNumber.Contains(search)) ||
                (x.Description != null && x.Description.Contains(search)));
        }

        if (query.CourseId.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.CourseId == query.CourseId.Value);
        }

        if (query.PartnerOrganizationId.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.PartnerOrganizationId == query.PartnerOrganizationId.Value);
        }

        if (query.IsActive.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.IsActive == query.IsActive.Value);
        }

        if (query.StartFrom.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.StartDate.HasValue &&
                x.StartDate.Value >= query.StartFrom.Value.Date);
        }

        if (query.StartTo.HasValue)
        {
            var endExclusive = query.StartTo.Value.Date.AddDays(1);

            itemsQuery = itemsQuery.Where(x =>
                x.StartDate.HasValue &&
                x.StartDate.Value < endExclusive);
        }

        if (query.EndFrom.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.EndDate.HasValue &&
                x.EndDate.Value >= query.EndFrom.Value.Date);
        }

        if (query.EndTo.HasValue)
        {
            var endExclusive = query.EndTo.Value.Date.AddDays(1);

            itemsQuery = itemsQuery.Where(x =>
                x.EndDate.HasValue &&
                x.EndDate.Value < endExclusive);
        }

        if (query.MinAgreedPrice.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.AgreedPrice.HasValue &&
                x.AgreedPrice.Value >= query.MinAgreedPrice.Value);
        }

        if (query.MaxAgreedPrice.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.AgreedPrice.HasValue &&
                x.AgreedPrice.Value <= query.MaxAgreedPrice.Value);
        }

        var totalCount = await itemsQuery.CountAsync();

        var rows = await itemsQuery
            .Select(x => new CoursePartnerOrganizationReportItemDto
            {
                Id = x.Id,
                CourseId = x.CourseId,
                CourseTitle = x.Course != null
                    ? x.Course.Title
                    : string.Empty,

                PartnerOrganizationId = x.PartnerOrganizationId,

                PartnerOrganizationName = x.PartnerOrganization != null
                    ? x.PartnerOrganization.Name
                    : string.Empty,

                ContractNumber = x.ContractNumber,
                AgreedPrice = x.AgreedPrice,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsActive = x.IsActive,
                Description = x.Description
            })
            .ToListAsync();

        foreach (var item in rows)
        {
            item.ContractStatus = GetContractStatus(
                item.IsActive,
                item.StartDate,
                item.EndDate);
        }

        rows = query.SortBy?.ToLowerInvariant() switch
        {
            "coursename" or "coursetitle" => query.SortDescending
                ? rows.OrderByDescending(x => x.CourseTitle).ToList()
                : rows.OrderBy(x => x.CourseTitle).ToList(),

            "partnerorganization" or "partnerorganizationname" => query.SortDescending
                ? rows.OrderByDescending(x => x.PartnerOrganizationName).ToList()
                : rows.OrderBy(x => x.PartnerOrganizationName).ToList(),

            "contractnumber" => query.SortDescending
                ? rows.OrderByDescending(x => x.ContractNumber).ToList()
                : rows.OrderBy(x => x.ContractNumber).ToList(),

            "agreedprice" => query.SortDescending
                ? rows.OrderByDescending(x => x.AgreedPrice).ToList()
                : rows.OrderBy(x => x.AgreedPrice).ToList(),

            "startdate" => query.SortDescending
                ? rows.OrderByDescending(x => x.StartDate).ToList()
                : rows.OrderBy(x => x.StartDate).ToList(),

            "enddate" => query.SortDescending
                ? rows.OrderByDescending(x => x.EndDate).ToList()
                : rows.OrderBy(x => x.EndDate).ToList(),

            "status" or "contractstatus" => query.SortDescending
                ? rows.OrderByDescending(x => x.ContractStatus).ToList()
                : rows.OrderBy(x => x.ContractStatus).ToList(),

            _ => query.SortDescending
                ? rows.OrderByDescending(x => x.Id).ToList()
                : rows.OrderBy(x => x.Id).ToList()
        };

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)query.PageSize);

        var items = rows
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var result = new CoursePartnerOrganizationReportResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };

        return Ok(result);
    }

    private static string GetContractStatus(
        bool isActive,
        DateTime? startDate,
        DateTime? endDate)
    {
        if (!isActive)
            return "غیرفعال";

        var today = DateTime.Now.Date;

        if (startDate.HasValue &&
            startDate.Value.Date > today)
        {
            return "آتی";
        }

        if (endDate.HasValue &&
            endDate.Value.Date < today)
        {
            return "منقضی";
        }

        return "فعال";
    }
}
