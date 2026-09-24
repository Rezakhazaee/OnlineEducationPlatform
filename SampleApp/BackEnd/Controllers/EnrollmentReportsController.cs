using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,EducationStaff")]
public class EnrollmentReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EnrollmentReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<EnrollmentReportResultDto>> Get(
        [FromQuery] EnrollmentReportQueryDto query)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = Math.Clamp(query.PageSize, 5, 100);

        var enrollments = _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
                .ThenInclude(s => s!.MarketingUser)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Instructor)
            .Include(e => e.SupportUser)
            .Include(e => e.Instructor)
            .Include(e => e.CoursePartnerOrganization)
                .ThenInclude(c => c!.PartnerOrganization)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            enrollments = enrollments.Where(e =>
                (e.Student != null &&
                 (e.Student.FirstName + " " + e.Student.LastName)
                    .Contains(search)) ||
                (e.Course != null &&
                 e.Course.Title.Contains(search)));
        }

        if (query.SupportUserId.HasValue)
            enrollments = enrollments.Where(
                e => e.SupportUserId == query.SupportUserId.Value);

        if (query.InstructorId.HasValue)
        {
            enrollments = enrollments.Where(e =>
                e.InstructorId == query.InstructorId.Value ||
                (e.Course != null &&
                 e.Course.InstructorId == query.InstructorId.Value));
        }

        if (query.MarketingUserId.HasValue)
        {
            enrollments = enrollments.Where(e =>
                e.Student != null &&
                e.Student.MarketingUserId == query.MarketingUserId.Value);
        }

        if (query.PartnerOrganizationId.HasValue)
        {
            enrollments = enrollments.Where(e =>
                e.CoursePartnerOrganization != null &&
                e.CoursePartnerOrganization.PartnerOrganizationId
                    == query.PartnerOrganizationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
            enrollments = enrollments.Where(
                e => e.Status == query.Status);

        if (query.StartFrom.HasValue)
        {
            enrollments = enrollments.Where(
                e => e.StartDate >= query.StartFrom.Value);
        }

        if (query.StartTo.HasValue)
        {
            var endDate = query.StartTo.Value.Date.AddDays(1);

            enrollments = enrollments.Where(
                e => e.StartDate < endDate);
        }

        var rows = await enrollments
            .Select(e => new
            {
                Enrollment = e,
                CoursePrice =
                    e.CoursePartnerOrganization != null &&
                    e.CoursePartnerOrganization.AgreedPrice.HasValue
                        ? e.CoursePartnerOrganization.AgreedPrice.Value
                        : e.Course != null
                            ? e.Course.Price
                            : 0,

                TotalPaid = _context.Payments
                    .Where(p =>
                        p.EnrollmentId == e.Id &&
                        p.Status == "Paid")
                    .Select(p => (decimal?)p.Amount)
                    .Sum() ?? 0
            })
            .ToListAsync();

        var items = rows
            .Select(x => MapItem(
                x.Enrollment,
                x.CoursePrice,
                x.TotalPaid))
            .ToList();

        if (query.MinPrice.HasValue)
        {
            items = items
                .Where(x => x.CoursePrice >= query.MinPrice.Value)
                .ToList();
        }

        if (query.MaxPrice.HasValue)
        {
            items = items
                .Where(x => x.CoursePrice <= query.MaxPrice.Value)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(query.PaymentStatus))
        {
            items = items
                .Where(x => x.PaymentStatus == query.PaymentStatus)
                .ToList();
        }

        items = SortItems(
            items,
            query.SortBy,
            query.SortDescending);

        var totalCount = items.Count;

        var pageItems = items
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return Ok(new EnrollmentReportResultDto
        {
            Items = pageItems,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)query.PageSize)
        });
    }

    private static EnrollmentReportItemDto MapItem(
        Enrollment e,
        decimal coursePrice,
        decimal totalPaid)
    {
        return new EnrollmentReportItemDto
        {
            Id = e.Id,
            StudentId = e.StudentId,
            StudentName = e.Student != null
                ? $"{e.Student.FirstName} {e.Student.LastName}"
                : string.Empty,

            CourseId = e.CourseId,
            CourseTitle = e.Course?.Title ?? string.Empty,

            CoursePrice = coursePrice,
            TotalPaid = totalPaid,
            RemainingAmount = Math.Max(coursePrice - totalPaid, 0),
            PaymentStatus = GetPaymentStatus(
                coursePrice,
                totalPaid),

            SupportUserId = e.SupportUserId,
            SupportUserName = e.SupportUser?.FullName,

            InstructorId =
                e.InstructorId ??
                e.Course?.InstructorId,

            InstructorName =
                e.Instructor?.FullName ??
                e.Course?.Instructor?.FullName,

            MarketingUserId =
                e.Student?.MarketingUserId,

            MarketingUserName =
                e.Student?.MarketingUser?.FullName,

            PartnerOrganizationId =
                e.CoursePartnerOrganization?.PartnerOrganizationId,

            PartnerOrganizationName =
                e.CoursePartnerOrganization?.PartnerOrganization?.Name,

            StartDate = e.StartDate,
            Status = e.Status,
            Description = e.Description
        };
    }

    private static string GetPaymentStatus(
        decimal price,
        decimal paid)
    {
        if (price <= 0)
            return "Paid";

        if (paid <= 0)
            return "Unpaid";

        if (paid < price)
            return "PartiallyPaid";

        if (paid == price)
            return "Paid";

        return "Overpaid";
    }

    private static List<EnrollmentReportItemDto> SortItems(
        List<EnrollmentReportItemDto> items,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "student" => descending
                ? items.OrderByDescending(x => x.StudentName).ToList()
                : items.OrderBy(x => x.StudentName).ToList(),

            "course" => descending
                ? items.OrderByDescending(x => x.CourseTitle).ToList()
                : items.OrderBy(x => x.CourseTitle).ToList(),

            "price" => descending
                ? items.OrderByDescending(x => x.CoursePrice).ToList()
                : items.OrderBy(x => x.CoursePrice).ToList(),

            "paid" => descending
                ? items.OrderByDescending(x => x.TotalPaid).ToList()
                : items.OrderBy(x => x.TotalPaid).ToList(),

            "remaining" => descending
                ? items.OrderByDescending(x => x.RemainingAmount).ToList()
                : items.OrderBy(x => x.RemainingAmount).ToList(),

            "paymentstatus" => descending
                ? items.OrderByDescending(x => x.PaymentStatus).ToList()
                : items.OrderBy(x => x.PaymentStatus).ToList(),

            "status" => descending
                ? items.OrderByDescending(x => x.Status).ToList()
                : items.OrderBy(x => x.Status).ToList(),

            _ => descending
                ? items.OrderByDescending(x => x.StartDate).ToList()
                : items.OrderBy(x => x.StartDate).ToList()
        };
    }
}
