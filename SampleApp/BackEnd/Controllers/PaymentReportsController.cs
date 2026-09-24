using BackEnd.Data;
using BackEnd.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,EducationStaff")]
public class PaymentReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PaymentReportResultDto>> Get(
        [FromQuery] PaymentReportQueryDto query)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = Math.Clamp(query.PageSize, 5, 100);

        var payments = _context.Payments
            .AsNoTracking()
            .Include(p => p.Enrollment)
                .ThenInclude(e => e!.Student)
                    .ThenInclude(s => s!.MarketingUser)
            .Include(p => p.Enrollment)
                .ThenInclude(e => e!.Course)
                    .ThenInclude(c => c!.Instructor)
            .Include(p => p.Enrollment)
                .ThenInclude(e => e!.SupportUser)
            .Include(p => p.Enrollment)
                .ThenInclude(e => e!.Instructor)
            .Include(p => p.Enrollment)
                .ThenInclude(e => e!.CoursePartnerOrganization)
                    .ThenInclude(c => c!.PartnerOrganization)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            payments = payments.Where(p =>
                (p.Enrollment != null &&
                 p.Enrollment.Student != null &&
                 (p.Enrollment.Student.FirstName + " " +
                  p.Enrollment.Student.LastName).Contains(search))
                ||
                (p.Enrollment != null &&
                 p.Enrollment.Course != null &&
                 p.Enrollment.Course.Title.Contains(search)));
        }

        if (query.SupportUserId.HasValue)
        {
            payments = payments.Where(p =>
                p.Enrollment != null &&
                p.Enrollment.SupportUserId ==
                    query.SupportUserId.Value);
        }

        if (query.MarketingUserId.HasValue)
        {
            payments = payments.Where(p =>
                p.Enrollment != null &&
                p.Enrollment.Student != null &&
                p.Enrollment.Student.MarketingUserId ==
                    query.MarketingUserId.Value);
        }

        if (query.InstructorId.HasValue)
        {
            payments = payments.Where(p =>
                p.Enrollment != null &&
                (
                    p.Enrollment.InstructorId ==
                        query.InstructorId.Value
                    ||
                    (p.Enrollment.Course != null &&
                     p.Enrollment.Course.InstructorId ==
                        query.InstructorId.Value)
                ));
        }

        if (query.PartnerOrganizationId.HasValue)
        {
            payments = payments.Where(p =>
                p.Enrollment != null &&
                p.Enrollment.CoursePartnerOrganization != null &&
                p.Enrollment.CoursePartnerOrganization
                    .PartnerOrganizationId ==
                    query.PartnerOrganizationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            payments = payments.Where(p =>
                p.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.PaymentType))
        {
            payments = payments.Where(p =>
                p.PaymentType == query.PaymentType);
        }

        if (!string.IsNullOrWhiteSpace(query.PaymentMethod))
        {
            payments = payments.Where(p =>
                p.PaymentMethod == query.PaymentMethod);
        }

        if (!string.IsNullOrWhiteSpace(query.EnrollmentStatus))
        {
            payments = payments.Where(p =>
                p.Enrollment != null &&
                p.Enrollment.Status == query.EnrollmentStatus);
        }

        if (query.PaymentFrom.HasValue)
        {
            payments = payments.Where(p =>
                p.PaymentDate >= query.PaymentFrom.Value);
        }

        if (query.PaymentTo.HasValue)
        {
            var endDate = query.PaymentTo.Value.Date.AddDays(1);

            payments = payments.Where(p =>
                p.PaymentDate < endDate);
        }

        if (query.MinAmount.HasValue)
        {
            payments = payments.Where(p =>
                p.Amount >= query.MinAmount.Value);
        }

        if (query.MaxAmount.HasValue)
        {
            payments = payments.Where(p =>
                p.Amount <= query.MaxAmount.Value);
        }

        var totalCount = await payments.CountAsync();

        var totalAmount = await payments.SumAsync(
            p => (decimal?)p.Amount) ?? 0;

        payments = query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "student" => query.SortDescending
                ? payments.OrderByDescending(
                    p => p.Enrollment!.Student!.LastName)
                : payments.OrderBy(
                    p => p.Enrollment!.Student!.LastName),

            "course" => query.SortDescending
                ? payments.OrderByDescending(
                    p => p.Enrollment!.Course!.Title)
                : payments.OrderBy(
                    p => p.Enrollment!.Course!.Title),

            "amount" => query.SortDescending
                ? payments.OrderByDescending(p => p.Amount)
                : payments.OrderBy(p => p.Amount),

            "status" => query.SortDescending
                ? payments.OrderByDescending(p => p.Status)
                : payments.OrderBy(p => p.Status),

            "paymenttype" => query.SortDescending
                ? payments.OrderByDescending(p => p.PaymentType)
                : payments.OrderBy(p => p.PaymentType),

            _ => query.SortDescending
                ? payments.OrderByDescending(p => p.PaymentDate)
                : payments.OrderBy(p => p.PaymentDate)
        };

        var entities = await payments
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var items = entities
            .Select(MapItem)
            .ToList();

        return Ok(new PaymentReportResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)query.PageSize),
            TotalAmount = totalAmount
        });
    }

    private static PaymentReportItemDto MapItem(
        BackEnd.Models.Payment payment)
    {
        var enrollment = payment.Enrollment;

        return new PaymentReportItemDto
        {
            Id = payment.Id,
            EnrollmentId = payment.EnrollmentId,

            StudentName =
                enrollment?.Student == null
                    ? string.Empty
                    : $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",

            CourseTitle =
                enrollment?.Course?.Title ?? string.Empty,

            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentType = payment.PaymentType,
            PaymentMethod = payment.PaymentMethod,
            GatewayRefId = payment.GatewayRefId,
            Status = payment.Status,

            EnrollmentStatus =
                enrollment?.Status ?? string.Empty,

            SupportUserName =
                enrollment?.SupportUser?.FullName,

            MarketingUserName =
                enrollment?.Student?.MarketingUser?.FullName,

            InstructorName =
                enrollment?.Instructor?.FullName ??
                enrollment?.Course?.Instructor?.FullName,

            PartnerOrganizationName =
                enrollment?.CoursePartnerOrganization?
                    .PartnerOrganization?.Name,

            Description = payment.Description
        };
    }
}
