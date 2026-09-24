using BackEnd.Data;
using BackEnd.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,EducationStaff")]
public class PaymentReportExportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentReportExportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("excel")]
    public async Task<IActionResult> Excel(
        [FromQuery] PaymentReportQueryDto query)
    {
        var items = await BuildItems(query);

        using var workbook = new XLWorkbook();

        var sheet = workbook.Worksheets.Add("گزارش پرداخت‌ها");
        sheet.RightToLeft = true;

        sheet.Cell(1, 1).Value = "گزارش پرداخت‌ها";
        sheet.Cell(1, 1).Style.Font.Bold = true;

        sheet.Cell(2, 1).Value = "مجموع مبلغ:";
        sheet.Cell(2, 2).Value = items.Sum(x => x.Amount);
        sheet.Cell(2, 2).Style.NumberFormat.Format = "#,##0";

        string[] headers =
        {
            "ردیف",
            "دانشجو",
            "دوره",
            "مبلغ",
            "تاریخ پرداخت",
            "نوع پرداخت",
            "روش پرداخت",
            "وضعیت پرداخت",
            "وضعیت ثبت‌نام",
            "پشتیبان",
            "مدرس",
            "بازاریاب",
            "سازمان طرف قرارداد",
            "شناسه درگاه"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(4, i + 1).Value = headers[i];
            sheet.Cell(4, i + 1).Style.Font.Bold = true;
        }

        for (var i = 0; i < items.Count; i++)
        {
            var row = i + 5;
            var item = items[i];

            sheet.Cell(row, 1).Value = i + 1;
            sheet.Cell(row, 2).Value = item.StudentName;
            sheet.Cell(row, 3).Value = item.CourseTitle;
            sheet.Cell(row, 4).Value = item.Amount;
            sheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
            sheet.Cell(row, 5).Value =
                item.PaymentDate.ToString("yyyy/MM/dd HH:mm");
            sheet.Cell(row, 6).Value =
                GetPaymentType(item.PaymentType);
            sheet.Cell(row, 7).Value =
                item.PaymentMethod ?? "نامشخص";
            sheet.Cell(row, 8).Value =
                GetPaymentStatus(item.Status);
            sheet.Cell(row, 9).Value =
                GetEnrollmentStatus(item.EnrollmentStatus);
            sheet.Cell(row, 10).Value =
                item.SupportUserName ?? "بدون پشتیبان";
            sheet.Cell(row, 11).Value =
                item.InstructorName ?? "بدون مدرس";
            sheet.Cell(row, 12).Value =
                item.MarketingUserName ?? "بدون بازاریاب";
            sheet.Cell(row, 13).Value =
                item.PartnerOrganizationName ?? "بدون سازمان";
            sheet.Cell(row, 14).Value =
                item.GatewayRefId ?? "";
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "payment-report.xlsx");
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> Pdf(
        [FromQuery] PaymentReportQueryDto query)
    {
        var items = await BuildItems(query);

        var totalAmount = items.Sum(x => x.Amount);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.ContentFromRightToLeft();

                page.DefaultTextStyle(style =>
                    style.FontFamily("Noto Sans Arabic")
                        .FontSize(7));

                page.Header()
                    .AlignCenter()
                    .Text("گزارش پرداخت‌ها")
                    .Bold()
                    .FontSize(15);

                page.Content()
                    .PaddingTop(10)
                    .Column(column =>
                    {
                        column.Item()
                            .AlignRight()
                            .Text(
                                $"مجموع پرداخت‌ها: {totalAmount:N0} تومان")
                            .Bold();

                        column.Item()
                            .PaddingTop(10)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(28);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1.7f);
                                    columns.ConstantColumn(75);
                                    columns.ConstantColumn(70);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1.0f);
                                    columns.ConstantColumn(65);
                                    columns.ConstantColumn(65);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1.1f);
                                });

                                foreach (var header in new[]
                                {
                                    "ردیف",
                                    "دانشجو",
                                    "دوره",
                                    "مبلغ",
                                    "تاریخ",
                                    "نوع",
                                    "روش",
                                    "وضعیت",
                                    "ثبت‌نام",
                                    "پشتیبان",
                                    "مدرس",
                                    "بازاریاب"
                                })
                                {
                                    HeaderCell(table.Cell(), header);
                                }

                                foreach (var item in items)
                                {
                                    BodyCell(
                                        table.Cell(),
                                        item.Id.ToString());

                                    BodyCell(
                                        table.Cell(),
                                        item.StudentName);

                                    BodyCell(
                                        table.Cell(),
                                        item.CourseTitle);

                                    BodyCell(
                                        table.Cell(),
                                        item.Amount.ToString("N0"));

                                    BodyCell(
                                        table.Cell(),
                                        item.PaymentDate
                                            .ToString("yyyy/MM/dd"));

                                    BodyCell(
                                        table.Cell(),
                                        GetPaymentType(
                                            item.PaymentType));

                                    BodyCell(
                                        table.Cell(),
                                        item.PaymentMethod
                                            ?? "نامشخص");

                                    BodyCell(
                                        table.Cell(),
                                        GetPaymentStatus(
                                            item.Status));

                                    BodyCell(
                                        table.Cell(),
                                        GetEnrollmentStatus(
                                            item.EnrollmentStatus));

                                    BodyCell(
                                        table.Cell(),
                                        item.SupportUserName
                                            ?? "بدون پشتیبان");

                                    BodyCell(
                                        table.Cell(),
                                        item.InstructorName
                                            ?? "بدون مدرس");

                                    BodyCell(
                                        table.Cell(),
                                        item.MarketingUserName
                                            ?? "بدون بازاریاب");
                                }
                            });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("آموزش‌یار - گزارش پرداخت‌ها");
            });
        });

        return File(
            document.GeneratePdf(),
            "application/pdf",
            "payment-report.pdf");
    }

    private async Task<List<PaymentReportItemDto>> BuildItems(
        PaymentReportQueryDto query)
    {
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
                  p.Enrollment.Student.LastName)
                    .Contains(search))
                ||
                (p.Enrollment != null &&
                 p.Enrollment.Course != null &&
                 p.Enrollment.Course.Title.Contains(search)));
        }

        if (query.SupportUserId.HasValue)
            payments = payments.Where(p =>
                p.Enrollment != null &&
                p.Enrollment.SupportUserId ==
                    query.SupportUserId.Value);

        if (query.MarketingUserId.HasValue)
            payments = payments.Where(p =>
                p.Enrollment != null &&
                p.Enrollment.Student != null &&
                p.Enrollment.Student.MarketingUserId ==
                    query.MarketingUserId.Value);

        if (query.InstructorId.HasValue)
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

        if (query.PartnerOrganizationId.HasValue)
            payments = payments.Where(p =>
                p.Enrollment != null &&
                p.Enrollment.CoursePartnerOrganization != null &&
                p.Enrollment.CoursePartnerOrganization
                    .PartnerOrganizationId ==
                    query.PartnerOrganizationId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            payments = payments.Where(
                p => p.Status == query.Status);

        if (!string.IsNullOrWhiteSpace(query.PaymentType))
            payments = payments.Where(
                p => p.PaymentType == query.PaymentType);

        if (!string.IsNullOrWhiteSpace(query.PaymentMethod))
            payments = payments.Where(
                p => p.PaymentMethod == query.PaymentMethod);

        if (!string.IsNullOrWhiteSpace(query.EnrollmentStatus))
            payments = payments.Where(
                p => p.Enrollment != null &&
                     p.Enrollment.Status ==
                     query.EnrollmentStatus);

        if (query.PaymentFrom.HasValue)
            payments = payments.Where(
                p => p.PaymentDate >= query.PaymentFrom.Value);

        if (query.PaymentTo.HasValue)
        {
            var endDate = query.PaymentTo.Value
                .Date
                .AddDays(1);

            payments = payments.Where(
                p => p.PaymentDate < endDate);
        }

        if (query.MinAmount.HasValue)
            payments = payments.Where(
                p => p.Amount >= query.MinAmount.Value);

        if (query.MaxAmount.HasValue)
            payments = payments.Where(
                p => p.Amount <= query.MaxAmount.Value);

        payments = query.SortBy?.Trim().ToLowerInvariant()
            switch
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

        var entities = await payments.ToListAsync();

        return entities.Select(p => new PaymentReportItemDto
        {
            Id = p.Id,
            EnrollmentId = p.EnrollmentId,

            StudentName =
                p.Enrollment?.Student == null
                    ? string.Empty
                    : $"{p.Enrollment.Student.FirstName} {p.Enrollment.Student.LastName}",

            CourseTitle =
                p.Enrollment?.Course?.Title ?? string.Empty,

            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            PaymentType = p.PaymentType,
            PaymentMethod = p.PaymentMethod,
            GatewayRefId = p.GatewayRefId,
            Status = p.Status,

            EnrollmentStatus =
                p.Enrollment?.Status ?? string.Empty,

            SupportUserName =
                p.Enrollment?.SupportUser?.FullName,

            MarketingUserName =
                p.Enrollment?.Student?.MarketingUser?.FullName,

            InstructorName =
                p.Enrollment?.Instructor?.FullName ??
                p.Enrollment?.Course?.Instructor?.FullName,

            PartnerOrganizationName =
                p.Enrollment?.CoursePartnerOrganization?
                    .PartnerOrganization?.Name,

            Description = p.Description
        }).ToList();
    }

    private static string GetPaymentStatus(string value) =>
        value switch
        {
            "Paid" => "تسویه‌شده",
            "Pending" => "در انتظار",
            "Cancelled" => "لغوشده",
            _ => value
        };

    private static string GetPaymentType(string value) =>
        value switch
        {
            "FirstInstallment" => "قسط اول",
            "SecondInstallment" => "قسط دوم",
            "ThirdInstallment" => "قسط سوم",
            "FullPayment" => "پرداخت کامل",
            _ => value
        };

    private static string GetEnrollmentStatus(string value) =>
        value switch
        {
            "Active" => "فعال",
            "Completed" => "تکمیل‌شده",
            "Cancelled" => "لغوشده",
            "Pending" => "در انتظار",
            "Suspended" => "معلق",
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
