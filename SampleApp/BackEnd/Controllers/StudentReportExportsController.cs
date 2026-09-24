using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using BackEnd.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,EducationStaff,Marketer,Support,Instructor")]
public class StudentReportExportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PackageAccessService _packageAccess;

    public StudentReportExportsController(
        ApplicationDbContext context,
        PackageAccessService packageAccess)
    {
        _context = context;
        _packageAccess = packageAccess;
    }

    [HttpGet("excel")]
    public async Task<IActionResult> Excel(
        [FromQuery] StudentReportQueryDto request)
    {
        var query = await BuildQuery(request);

        if (query.Error != null)
            return query.Error;

        var rows = await query.Query!
            .Select(s => new StudentExportRow
            {
                Id = s.Id,
                FullName = (s.FirstName + " " + s.LastName).Trim(),
                NationalCode = s.NationalCode,
                Mobile = s.Mobile,
                MarketingUserName = s.MarketingUser != null
                    ? s.MarketingUser.FullName
                    : "بدون تخصیص",
                SupportUserName = s.SupportUser != null
                    ? s.SupportUser.FullName
                    : "بدون تخصیص",
                PartnerOrganizationName = s.PartnerOrganization != null
                    ? s.PartnerOrganization.Name
                    : "بدون سازمان",
                CreatedDate = s.CreatedDate
            })
            .ToListAsync();

        using var workbook = new XLWorkbook();

        workbook.RightToLeft = true;

        var sheet = workbook.AddWorksheet("دانشجویان");
        sheet.RightToLeft = true;

        var headers = new[]
        {
            "شناسه",
            "دانشجو",
            "کد ملی",
            "موبایل",
            "بازاریاب",
            "پشتیبان",
            "سازمان طرف قرارداد",
            "تاریخ ایجاد"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
        }

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var excelRow = i + 2;

            sheet.Cell(excelRow, 1).Value = row.Id;
            sheet.Cell(excelRow, 2).Value = row.FullName;
            sheet.Cell(excelRow, 3).Value = row.NationalCode;
            sheet.Cell(excelRow, 4).Value = row.Mobile;
            sheet.Cell(excelRow, 5).Value = row.MarketingUserName;
            sheet.Cell(excelRow, 6).Value = row.SupportUserName;
            sheet.Cell(excelRow, 7).Value =
                row.PartnerOrganizationName;
            sheet.Cell(excelRow, 8).Value =
                row.CreatedDate.ToString("yyyy/MM/dd");
        }

        var headerRange = sheet.Range(
            1,
            1,
            1,
            headers.Length);

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#568fa8");

        var usedRange = sheet.RangeUsed();

        if (usedRange != null)
        {
            usedRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            usedRange.Style.Alignment.WrapText = true;
            usedRange.SetAutoFilter();
        }

        sheet.SheetView.FreezeRows(1);

        sheet.Column(1).Width = 10;
        sheet.Column(2).Width = 25;
        sheet.Column(3).Width = 18;
        sheet.Column(4).Width = 18;
        sheet.Column(5).Width = 24;
        sheet.Column(6).Width = 24;
        sheet.Column(7).Width = 28;
        sheet.Column(8).Width = 18;

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"student-report-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> Pdf(
        [FromQuery] StudentReportQueryDto request)
    {
        var query = await BuildQuery(request);

        if (query.Error != null)
            return query.Error;

        var rows = await query.Query!
            .Select(s => new StudentExportRow
            {
                Id = s.Id,
                FullName = (s.FirstName + " " + s.LastName).Trim(),
                NationalCode = s.NationalCode,
                Mobile = s.Mobile,
                MarketingUserName = s.MarketingUser != null
                    ? s.MarketingUser.FullName
                    : "بدون تخصیص",
                SupportUserName = s.SupportUser != null
                    ? s.SupportUser.FullName
                    : "بدون تخصیص",
                PartnerOrganizationName = s.PartnerOrganization != null
                    ? s.PartnerOrganization.Name
                    : "بدون سازمان",
                CreatedDate = s.CreatedDate
            })
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(style => style.FontFamily("Lato", "Noto Sans Arabic").FontSize(8));

                page.Header()
                    .AlignCenter()
                    .Text("گزارش دانشجویان")
                    .Bold()
                    .FontSize(16);

                page.Content()
                    .PaddingTop(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.8f);
                            columns.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            HeaderCell(header.Cell(), "شناسه");
                            HeaderCell(header.Cell(), "دانشجو");
                            HeaderCell(header.Cell(), "کد ملی");
                            HeaderCell(header.Cell(), "موبایل");
                            HeaderCell(header.Cell(), "بازاریاب");
                            HeaderCell(header.Cell(), "پشتیبان");
                            HeaderCell(header.Cell(), "سازمان طرف قرارداد");
                            HeaderCell(header.Cell(), "تاریخ ایجاد");
                        });

                        foreach (var row in rows)
                        {
                            BodyCell(table.Cell(), row.Id.ToString());
                            BodyCell(table.Cell(), row.FullName);
                            BodyCell(table.Cell(), row.NationalCode);
                            BodyCell(table.Cell(), row.Mobile);
                            BodyCell(table.Cell(), row.MarketingUserName);
                            BodyCell(table.Cell(), row.SupportUserName);
                            BodyCell(table.Cell(),
                                row.PartnerOrganizationName);
                            BodyCell(table.Cell(),
                                row.CreatedDate.ToString("yyyy/MM/dd"));
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("صفحه ");
                        text.CurrentPageNumber();
                        text.Span(" از ");
                        text.TotalPages();
                    });
            });
        });

        var pdfBytes = document.GeneratePdf();

        return File(
            pdfBytes,
            "application/pdf",
            $"student-report-{DateTime.Now:yyyyMMdd-HHmm}.pdf");
    }

    private async Task<(
        IQueryable<Student>? Query,
        IActionResult? Error)> BuildQuery(
        StudentReportQueryDto request)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return (
                null,
                Unauthorized(new
                {
                    message = "شناسه کاربر معتبر نیست."
                }));
        }

        var currentUser =
            await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.Id == userId);

        if (currentUser == null)
        {
            return (
                null,
                Unauthorized(new
                {
                    message = "کاربر پیدا نشد."
                }));
        }

        if (request.PartnerOrganizationId.HasValue &&
            !await _packageAccess.HasPackageAsync(4))
        {
            return (
                null,
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "فیلتر سازمان طرف قرارداد فقط در پکیج سازمانی فعال است."
                    }));
        }

        IQueryable<Student> query =
            _context.Students.AsNoTracking();

        switch (currentUser.Role)
        {
            case "Admin":
            case "EducationStaff":
                break;

            case "Support":
                query = query.Where(
                    s => s.SupportUserId == currentUser.Id);
                break;

            case "Marketer":
                query = query.Where(
                    s => s.MarketingUserId == currentUser.Id);
                break;

            case "Instructor":
                query = query.Where(
                    s => _context.Enrollments.Any(
                        e =>
                            e.StudentId == s.Id &&
                            e.InstructorId == currentUser.Id));
                break;

            default:
                return (null, Forbid());
        }

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

        if (request.SupportUserId.HasValue)
        {
            query = query.Where(
                s => s.SupportUserId ==
                     request.SupportUserId.Value);
        }

        if (request.MarketingUserId.HasValue)
        {
            query = query.Where(
                s => s.MarketingUserId ==
                     request.MarketingUserId.Value);
        }

        if (request.PartnerOrganizationId.HasValue)
        {
            query = query.Where(
                s => s.PartnerOrganizationId ==
                     request.PartnerOrganizationId.Value);
        }

        if (request.CreatedFrom.HasValue)
        {
            var from = request.CreatedFrom.Value.Date;

            query = query.Where(
                s => s.CreatedDate >= from);
        }

        if (request.CreatedTo.HasValue)
        {
            var toExclusive =
                request.CreatedTo.Value.Date.AddDays(1);

            query = query.Where(
                s => s.CreatedDate < toExclusive);
        }

        query =
            request.SortBy?.Trim().ToLowerInvariant() switch
            {
                "firstname" =>
                    request.SortDescending
                        ? query.OrderByDescending(
                            s => s.FirstName)
                        : query.OrderBy(
                            s => s.FirstName),

                "lastname" =>
                    request.SortDescending
                        ? query.OrderByDescending(
                            s => s.LastName)
                        : query.OrderBy(
                            s => s.LastName),

                "mobile" =>
                    request.SortDescending
                        ? query.OrderByDescending(
                            s => s.Mobile)
                        : query.OrderBy(
                            s => s.Mobile),

                _ =>
                    request.SortDescending
                        ? query.OrderByDescending(
                            s => s.CreatedDate)
                        : query.OrderBy(
                            s => s.CreatedDate)
            };

        return (query, null);
    }

    private static void HeaderCell(
        IContainer cell,
        string text)
    {
        cell
            .Background("#568fa8")
            .Padding(4)
            .AlignCenter()
            .Text(text)
            .Bold()
            .FontColor(Colors.White);
    }

    private static void BodyCell(
        IContainer cell,
        string text)
    {
        cell
            .Padding(4)
            .BorderBottom(1)
            .BorderColor("#D9E1E7")
            .Text(text);
    }

    private sealed class StudentExportRow
    {
        public int Id { get; set; }

        public string FullName { get; set; } =
            string.Empty;

        public string NationalCode { get; set; } =
            string.Empty;

        public string Mobile { get; set; } =
            string.Empty;

        public string MarketingUserName { get; set; } =
            string.Empty;

        public string SupportUserName { get; set; } =
            string.Empty;

        public string PartnerOrganizationName { get; set; } =
            string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
