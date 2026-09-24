using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BackEnd.Data;
using BackEnd.DTOs;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,EducationStaff")]
public class PartnerOrganizationReportExportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PartnerOrganizationReportExportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] PartnerOrganizationReportQueryDto query)
    {
        var items = await BuildReport(query);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("سازمان‌های طرف قرارداد");

        var headers = new[]
        {
            "شناسه",
            "نام سازمان",
            "مسئول",
            "موبایل",
            "شماره قرارداد",
            "شروع قرارداد",
            "پایان قرارداد",
            "وضعیت سازمان",
            "تاریخ ایجاد",
            "تعداد دوره",
            "تعداد دانشجو",
            "وضعیت قرارداد"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var headerRange = worksheet.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        for (var rowIndex = 0; rowIndex < items.Count; rowIndex++)
        {
            var item = items[rowIndex];
            var row = rowIndex + 2;

            worksheet.Cell(row, 1).Value = item.Id;
            worksheet.Cell(row, 2).Value = item.Name;
            worksheet.Cell(row, 3).Value = item.ContactPerson ?? "";
            worksheet.Cell(row, 4).Value = item.ContactMobile ?? "";
            worksheet.Cell(row, 5).Value = item.ContractNumber ?? "";
            worksheet.Cell(row, 6).Value = item.ContractStartDate?.ToString("yyyy/MM/dd") ?? "";
            worksheet.Cell(row, 7).Value = item.ContractEndDate?.ToString("yyyy/MM/dd") ?? "";
            worksheet.Cell(row, 8).Value = item.IsActive ? "فعال" : "غیرفعال";
            worksheet.Cell(row, 9).Value = item.CreatedDate.ToString("yyyy/MM/dd");
            worksheet.Cell(row, 10).Value = item.CourseCount;
            worksheet.Cell(row, 11).Value = item.StudentCount;
            worksheet.Cell(row, 12).Value = item.ContractStatus;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "partner-organization-report.xlsx");
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] PartnerOrganizationReportQueryDto query)
    {
        var items = await BuildReport(query);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily("Noto Sans Arabic").FontSize(8));

                page.Header()
                    .Text("گزارش تفصیلی سازمان‌های طرف قرارداد")
                    .Bold()
                    .FontSize(14);

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("شناسه");
                        header.Cell().Element(HeaderCell).Text("نام سازمان");
                        header.Cell().Element(HeaderCell).Text("مسئول");
                        header.Cell().Element(HeaderCell).Text("شماره قرارداد");
                        header.Cell().Element(HeaderCell).Text("شروع");
                        header.Cell().Element(HeaderCell).Text("پایان");
                        header.Cell().Element(HeaderCell).Text("دوره");
                        header.Cell().Element(HeaderCell).Text("دانشجو");
                        header.Cell().Element(HeaderCell).Text("وضعیت");
                    });

                    foreach (var item in items)
                    {
                        table.Cell().Element(BodyCell).Text(item.Id.ToString());
                        table.Cell().Element(BodyCell).Text(item.Name);
                        table.Cell().Element(BodyCell).Text(item.ContactPerson ?? "-");
                        table.Cell().Element(BodyCell).Text(item.ContractNumber ?? "-");
                        table.Cell().Element(BodyCell).Text(
                            item.ContractStartDate?.ToString("yyyy/MM/dd") ?? "-");
                        table.Cell().Element(BodyCell).Text(
                            item.ContractEndDate?.ToString("yyyy/MM/dd") ?? "-");
                        table.Cell().Element(BodyCell).Text(item.CourseCount.ToString());
                        table.Cell().Element(BodyCell).Text(item.StudentCount.ToString());
                        table.Cell().Element(BodyCell).Text(item.ContractStatus);
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("تعداد رکوردها: ");
                        text.Span(items.Count.ToString());
                    });
            });
        });

        var pdfBytes = document.GeneratePdf();

        return File(
            pdfBytes,
            "application/pdf",
            "partner-organization-report.pdf");
    }

    private async Task<List<PartnerOrganizationReportItemDto>> BuildReport(
        PartnerOrganizationReportQueryDto query)
    {
        var organizations = _context.PartnerOrganizations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            organizations = organizations.Where(x =>
                x.Name.Contains(search) ||
                (x.ContactPerson != null && x.ContactPerson.Contains(search)) ||
                (x.ContactMobile != null && x.ContactMobile.Contains(search)) ||
                (x.ContractNumber != null && x.ContractNumber.Contains(search)));
        }

        if (query.IsActive.HasValue)
        {
            organizations = organizations.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            organizations = organizations.Where(x =>
                x.CreatedDate >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            var toDate = query.CreatedTo.Value.Date.AddDays(1);
            organizations = organizations.Where(x => x.CreatedDate < toDate);
        }

        if (query.ContractStartFrom.HasValue)
        {
            organizations = organizations.Where(x =>
                x.ContractStartDate.HasValue &&
                x.ContractStartDate.Value >= query.ContractStartFrom.Value);
        }

        if (query.ContractStartTo.HasValue)
        {
            var toDate = query.ContractStartTo.Value.Date.AddDays(1);
            organizations = organizations.Where(x =>
                x.ContractStartDate.HasValue &&
                x.ContractStartDate.Value < toDate);
        }

        if (query.ContractEndFrom.HasValue)
        {
            organizations = organizations.Where(x =>
                x.ContractEndDate.HasValue &&
                x.ContractEndDate.Value >= query.ContractEndFrom.Value);
        }

        if (query.ContractEndTo.HasValue)
        {
            var toDate = query.ContractEndTo.Value.Date.AddDays(1);
            organizations = organizations.Where(x =>
                x.ContractEndDate.HasValue &&
                x.ContractEndDate.Value < toDate);
        }

        var baseItems = await organizations
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.ContactPerson,
                x.ContactMobile,
                x.ContractNumber,
                x.ContractStartDate,
                x.ContractEndDate,
                x.IsActive,
                x.CreatedDate
            })
            .ToListAsync();

        var courseCounts = await _context.CoursePartnerOrganizations
            .GroupBy(x => x.PartnerOrganizationId)
            .Select(g => new
            {
                PartnerOrganizationId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.PartnerOrganizationId, x => x.Count);

        var studentCounts = await _context.Students
            .Where(x => x.PartnerOrganizationId.HasValue)
            .GroupBy(x => x.PartnerOrganizationId!.Value)
            .Select(g => new
            {
                PartnerOrganizationId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.PartnerOrganizationId, x => x.Count);

        var result = baseItems.Select(x => new PartnerOrganizationReportItemDto
        {
            Id = x.Id,
            Name = x.Name,
            ContactPerson = x.ContactPerson,
            ContactMobile = x.ContactMobile,
            ContractNumber = x.ContractNumber,
            ContractStartDate = x.ContractStartDate,
            ContractEndDate = x.ContractEndDate,
            IsActive = x.IsActive,
            CreatedDate = x.CreatedDate,
            CourseCount = courseCounts.TryGetValue(x.Id, out var courseCount)
                ? courseCount
                : 0,
            StudentCount = studentCounts.TryGetValue(x.Id, out var studentCount)
                ? studentCount
                : 0,
            ContractStatus = GetContractStatus(
                x.IsActive,
                x.ContractStartDate,
                x.ContractEndDate)
        }).ToList();

        result = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDescending
                ? result.OrderByDescending(x => x.Name).ToList()
                : result.OrderBy(x => x.Name).ToList(),

            "contractnumber" => query.SortDescending
                ? result.OrderByDescending(x => x.ContractNumber).ToList()
                : result.OrderBy(x => x.ContractNumber).ToList(),

            "contractstartdate" => query.SortDescending
                ? result.OrderByDescending(x => x.ContractStartDate).ToList()
                : result.OrderBy(x => x.ContractStartDate).ToList(),

            "contractenddate" => query.SortDescending
                ? result.OrderByDescending(x => x.ContractEndDate).ToList()
                : result.OrderBy(x => x.ContractEndDate).ToList(),

            "coursecount" => query.SortDescending
                ? result.OrderByDescending(x => x.CourseCount).ToList()
                : result.OrderBy(x => x.CourseCount).ToList(),

            "studentcount" => query.SortDescending
                ? result.OrderByDescending(x => x.StudentCount).ToList()
                : result.OrderBy(x => x.StudentCount).ToList(),

            _ => query.SortDescending
                ? result.OrderByDescending(x => x.CreatedDate).ToList()
                : result.OrderBy(x => x.CreatedDate).ToList()
        };

        return result;
    }

    private static string GetContractStatus(
        bool isActive,
        DateTime? startDate,
        DateTime? endDate)
    {
        if (!isActive)
            return "غیرفعال";

        var today = DateTime.Now.Date;

        if (startDate.HasValue && startDate.Value.Date > today)
            return "آتی";

        if (endDate.HasValue && endDate.Value.Date < today)
            return "منقضی";

        return "فعال";
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten2)
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .Padding(4)
            .AlignCenter();
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(4);
    }
}
