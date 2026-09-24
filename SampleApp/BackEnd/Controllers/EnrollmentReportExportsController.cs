using BackEnd.Data;
using BackEnd.DTOs;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,EducationStaff")]
public class EnrollmentReportExportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EnrollmentReportExportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("excel")]
    public async Task<IActionResult> Excel([FromQuery] EnrollmentReportQueryDto query)
    {
        var items = await BuildItems(query);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("گزارش ثبت‌نام‌ها");
        sheet.RightToLeft = true;

        sheet.Cell(1, 1).Value = "گزارش ثبت‌نام‌ها";
        sheet.Cell(1, 1).Style.Font.Bold = true;

        string[] headers =
        {
            "ردیف", "دانشجو", "دوره", "مدرس", "پشتیبان", "بازاریاب",
            "سازمان طرف قرارداد", "تاریخ شروع", "وضعیت ثبت‌نام",
            "قیمت", "پرداخت‌شده", "مانده", "وضعیت پرداخت"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(3, i + 1).Value = headers[i];
            sheet.Cell(3, i + 1).Style.Font.Bold = true;
        }

        for (var i = 0; i < items.Count; i++)
        {
            var row = i + 4;
            var item = items[i];

            sheet.Cell(row, 1).Value = i + 1;
            sheet.Cell(row, 2).Value = item.StudentName;
            sheet.Cell(row, 3).Value = item.CourseTitle;
            sheet.Cell(row, 4).Value = item.InstructorName ?? "بدون مدرس";
            sheet.Cell(row, 5).Value = item.SupportUserName ?? "بدون پشتیبان";
            sheet.Cell(row, 6).Value = item.MarketingUserName ?? "بدون بازاریاب";
            sheet.Cell(row, 7).Value = item.PartnerOrganizationName ?? "بدون سازمان";
            sheet.Cell(row, 8).Value = item.StartDate.ToString("yyyy/MM/dd");
            sheet.Cell(row, 9).Value = GetEnrollmentStatus(item.Status);
            sheet.Cell(row, 10).Value = item.CoursePrice;
            sheet.Cell(row, 11).Value = item.TotalPaid;
            sheet.Cell(row, 12).Value = item.RemainingAmount;
            sheet.Cell(row, 13).Value = GetPaymentStatus(item.PaymentStatus);

            for (var col = 10; col <= 12; col++)
                sheet.Cell(row, col).Style.NumberFormat.Format = "#,##0";
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "enrollment-report.xlsx");
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> Pdf([FromQuery] EnrollmentReportQueryDto query)
    {
        var items = await BuildItems(query);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.ContentFromRightToLeft();

                page.DefaultTextStyle(style =>
                    style.FontFamily("Noto Sans Arabic").FontSize(7));

                page.Header()
                    .AlignCenter()
                    .Text("گزارش ثبت‌نام‌ها")
                    .Bold()
                    .FontSize(15);

                page.Content()
                    .PaddingTop(12)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(28);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.7f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.3f);
                            columns.ConstantColumn(62);
                            columns.ConstantColumn(58);
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(65);
                        });

                        foreach (var header in new[]
                        {
                            "ردیف", "دانشجو", "دوره", "مدرس", "پشتیبان", "بازاریاب",
                            "سازمان", "تاریخ", "ثبت‌نام", "قیمت", "پرداخت", "مانده", "پرداخت"
                        })
                        {
                            HeaderCell(table.Cell(), header);
                        }

                        foreach (var item in items)
                        {
                            BodyCell(table.Cell(), item.Id.ToString());
                            BodyCell(table.Cell(), item.StudentName);
                            BodyCell(table.Cell(), item.CourseTitle);
                            BodyCell(table.Cell(), item.InstructorName ?? "بدون مدرس");
                            BodyCell(table.Cell(), item.SupportUserName ?? "بدون پشتیبان");
                            BodyCell(table.Cell(), item.MarketingUserName ?? "بدون بازاریاب");
                            BodyCell(table.Cell(), item.PartnerOrganizationName ?? "بدون سازمان");
                            BodyCell(table.Cell(), item.StartDate.ToString("yyyy/MM/dd"));
                            BodyCell(table.Cell(), GetEnrollmentStatus(item.Status));
                            BodyCell(table.Cell(), item.CoursePrice.ToString("N0"));
                            BodyCell(table.Cell(), item.TotalPaid.ToString("N0"));
                            BodyCell(table.Cell(), item.RemainingAmount.ToString("N0"));
                            BodyCell(table.Cell(), GetPaymentStatus(item.PaymentStatus));
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("آموزش‌یار - گزارش ثبت‌نام‌ها");
            });
        });

        return File(
            document.GeneratePdf(),
            "application/pdf",
            "enrollment-report.pdf");
    }

    private async Task<List<EnrollmentReportItemDto>> BuildItems(
        EnrollmentReportQueryDto query)
    {
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
            enrollments = enrollments.Where(e =>
                e.InstructorId == query.InstructorId.Value ||
                (e.Course != null &&
                 e.Course.InstructorId == query.InstructorId.Value));

        if (query.MarketingUserId.HasValue)
            enrollments = enrollments.Where(e =>
                e.Student != null &&
                e.Student.MarketingUserId == query.MarketingUserId.Value);

        if (query.PartnerOrganizationId.HasValue)
            enrollments = enrollments.Where(e =>
                e.CoursePartnerOrganization != null &&
                e.CoursePartnerOrganization.PartnerOrganizationId
                    == query.PartnerOrganizationId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            enrollments = enrollments.Where(
                e => e.Status == query.Status);

        if (query.StartFrom.HasValue)
            enrollments = enrollments.Where(
                e => e.StartDate >= query.StartFrom.Value);

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
            .Select(x => new EnrollmentReportItemDto
            {
                Id = x.Enrollment.Id,
                StudentId = x.Enrollment.StudentId,

                StudentName = x.Enrollment.Student != null
                    ? $"{x.Enrollment.Student.FirstName} {x.Enrollment.Student.LastName}"
                    : string.Empty,

                CourseId = x.Enrollment.CourseId,
                CourseTitle = x.Enrollment.Course?.Title ?? string.Empty,

                CoursePrice = x.CoursePrice,
                TotalPaid = x.TotalPaid,
                RemainingAmount =
                    Math.Max(x.CoursePrice - x.TotalPaid, 0),

                PaymentStatus =
                    GetPaymentStatusCode(
                        x.CoursePrice,
                        x.TotalPaid),

                SupportUserName =
                    x.Enrollment.SupportUser?.FullName,

                InstructorName =
                    x.Enrollment.Instructor?.FullName ??
                    x.Enrollment.Course?.Instructor?.FullName,

                MarketingUserName =
                    x.Enrollment.Student?.MarketingUser?.FullName,

                PartnerOrganizationName =
                    x.Enrollment.CoursePartnerOrganization?
                        .PartnerOrganization?.Name,

                StartDate = x.Enrollment.StartDate,
                Status = x.Enrollment.Status,
                Description = x.Enrollment.Description
            })
            .ToList();

        if (query.MinPrice.HasValue)
            items = items
                .Where(x => x.CoursePrice >= query.MinPrice.Value)
                .ToList();

        if (query.MaxPrice.HasValue)
            items = items
                .Where(x => x.CoursePrice <= query.MaxPrice.Value)
                .ToList();

        if (!string.IsNullOrWhiteSpace(query.PaymentStatus))
            items = items
                .Where(x => x.PaymentStatus == query.PaymentStatus)
                .ToList();

        return query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "student" =>
                query.SortDescending
                    ? items.OrderByDescending(x => x.StudentName).ToList()
                    : items.OrderBy(x => x.StudentName).ToList(),

            "course" =>
                query.SortDescending
                    ? items.OrderByDescending(x => x.CourseTitle).ToList()
                    : items.OrderBy(x => x.CourseTitle).ToList(),

            "price" =>
                query.SortDescending
                    ? items.OrderByDescending(x => x.CoursePrice).ToList()
                    : items.OrderBy(x => x.CoursePrice).ToList(),

            "paid" =>
                query.SortDescending
                    ? items.OrderByDescending(x => x.TotalPaid).ToList()
                    : items.OrderBy(x => x.TotalPaid).ToList(),

            "remaining" =>
                query.SortDescending
                    ? items.OrderByDescending(x => x.RemainingAmount).ToList()
                    : items.OrderBy(x => x.RemainingAmount).ToList(),

            "paymentstatus" =>
                query.SortDescending
                    ? items.OrderByDescending(x => x.PaymentStatus).ToList()
                    : items.OrderBy(x => x.PaymentStatus).ToList(),

            "status" =>
                query.SortDescending
                    ? items.OrderByDescending(x => x.Status).ToList()
                    : items.OrderBy(x => x.Status).ToList(),

            _ =>
                query.SortDescending
                    ? items.OrderByDescending(x => x.StartDate).ToList()
                    : items.OrderBy(x => x.StartDate).ToList()
        };
    }

    private static string GetPaymentStatusCode(
        decimal price,
        decimal paid)
    {
        if (price <= 0) return "Paid";
        if (paid <= 0) return "Unpaid";
        if (paid < price) return "PartiallyPaid";
        if (paid == price) return "Paid";

        return "Overpaid";
    }

    private static string GetPaymentStatus(string value) =>
        value switch
        {
            "Unpaid" => "پرداخت‌نشده",
            "PartiallyPaid" => "پرداخت ناقص",
            "Paid" => "تسویه‌شده",
            "Overpaid" => "بیش‌پرداخت",
            _ => value
        };

    private static string GetEnrollmentStatus(string value) =>
        value switch
        {
            "Active" => "فعال",
            "Completed" => "تکمیل‌شده",
            "Cancelled" => "لغوشده",
            "Pending" => "در انتظار",
            _ => value
        };

    private static void HeaderCell(
        IContainer cell,
        string text)
    {
        cell
            .Background("#263d4a")
            .Border(1)
            .BorderColor("#cccccc")
            .Padding(4)
            .AlignCenter()
            .Text(text)
            .Bold()
            .FontColor("#ffffff");
    }

    private static void BodyCell(
        IContainer cell,
        string text)
    {
        cell
            .Border(1)
            .BorderColor("#dddddd")
            .Padding(4)
            .AlignMiddle()
            .Text(text);
    }
}
