using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,EducationStaff,Marketer,Support,Instructor")]
public class StudentReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PackageAccessService _packageAccess;

    public StudentReportsController(
        ApplicationDbContext context,
        PackageAccessService packageAccess)
    {
        _context = context;
        _packageAccess = packageAccess;
    }

    [HttpGet]
    public async Task<ActionResult<StudentReportResultDto>> Get(
        [FromQuery] StudentReportQueryDto request)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست."
            });
        }

        var currentUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (currentUser == null)
        {
            return Unauthorized(new
            {
                message = "کاربر پیدا نشد."
            });
        }

        if (request.PartnerOrganizationId.HasValue &&
            !await _packageAccess.HasPackageAsync(4))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        "فیلتر سازمان طرف قرارداد فقط در پکیج سازمانی فعال است."
                });
        }

        IQueryable<Student> query =
            _context.Students.AsNoTracking();

        // محدوده دسترسی بر اساس نقش
        switch (currentUser.Role)
        {
            case "Admin":
            case "EducationStaff":
                break;

            case "Support":
                query = query.Where(s =>
                    s.SupportUserId == currentUser.Id);
                break;

            case "Marketer":
                query = query.Where(s =>
                    s.MarketingUserId == currentUser.Id);
                break;

            case "Instructor":
                query = query.Where(s =>
                    _context.Enrollments.Any(e =>
                        e.StudentId == s.Id &&
                        e.InstructorId == currentUser.Id));
                break;

            default:
                return Forbid();
        }

        // جستجو
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(s =>
                (s.FirstName + " " + s.LastName)
                    .Contains(search) ||
                s.FirstName.Contains(search) ||
                s.LastName.Contains(search) ||
                s.NationalCode.Contains(search) ||
                s.Mobile.Contains(search) ||
                (s.GuardianName ?? string.Empty)
                    .Contains(search) ||
                (s.GuardianMobile ?? string.Empty)
                    .Contains(search));
        }

        // فیلتر پشتیبان
        if (request.SupportUserId.HasValue)
        {
            query = query.Where(s =>
                s.SupportUserId == request.SupportUserId.Value);
        }

        // فیلتر بازاریاب
        if (request.MarketingUserId.HasValue)
        {
            query = query.Where(s =>
                s.MarketingUserId == request.MarketingUserId.Value);
        }

        // فیلتر سازمان
        if (request.PartnerOrganizationId.HasValue)
        {
            query = query.Where(s =>
                s.PartnerOrganizationId ==
                request.PartnerOrganizationId.Value);
        }

        // تاریخ ایجاد از
        if (request.CreatedFrom.HasValue)
        {
            var from = request.CreatedFrom.Value.Date;

            query = query.Where(s =>
                s.CreatedDate >= from);
        }

        // تاریخ ایجاد تا - شامل کل روز
        if (request.CreatedTo.HasValue)
        {
            var toExclusive =
                request.CreatedTo.Value.Date.AddDays(1);

            query = query.Where(s =>
                s.CreatedDate < toExclusive);
        }

        // تعداد کل نتایج بعد از فیلتر
        var totalCount =
            await query.CountAsync();

        // PageSize محدود برای جلوگیری از درخواست‌های سنگین
        var pageSize =
            Math.Clamp(request.PageSize, 5, 100);

        var totalPages =
            (int)Math.Ceiling(
                totalCount / (double)pageSize);

        var page =
            Math.Max(1, request.Page);

        if (totalPages > 0)
        {
            page = Math.Min(page, totalPages);
        }

        // مرتب‌سازی
        var sortBy =
            request.SortBy?
                .Trim()
                .ToLowerInvariant() ??
            "createddate";

        query = sortBy switch
        {
            "firstname" =>
                request.SortDescending
                    ? query
                        .OrderByDescending(s => s.FirstName)
                        .ThenByDescending(s => s.LastName)
                    : query
                        .OrderBy(s => s.FirstName)
                        .ThenBy(s => s.LastName),

            "lastname" =>
                request.SortDescending
                    ? query
                        .OrderByDescending(s => s.LastName)
                        .ThenByDescending(s => s.FirstName)
                    : query
                        .OrderBy(s => s.LastName)
                        .ThenBy(s => s.FirstName),

            "mobile" =>
                request.SortDescending
                    ? query.OrderByDescending(s => s.Mobile)
                    : query.OrderBy(s => s.Mobile),

            _ =>
                request.SortDescending
                    ? query.OrderByDescending(s => s.CreatedDate)
                    : query.OrderBy(s => s.CreatedDate)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                NationalCode = s.NationalCode,
                BirthDate = s.BirthDate,
                Mobile = s.Mobile,
                Address = s.Address,
                GuardianName = s.GuardianName,
                GuardianMobile = s.GuardianMobile,
                MarketingUserId = s.MarketingUserId,
                SupportUserId = s.SupportUserId,
                PartnerOrganizationId =
                    s.PartnerOrganizationId,
                CreatedDate = s.CreatedDate
            })
            .ToListAsync();

        return Ok(new StudentReportResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }
}
