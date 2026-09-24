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
public class CoursePartnerOrganizationReportExportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoursePartnerOrganizationReportExportsController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("excel")]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] CoursePartnerOrganizationReportQueryDto query)
    {
        var items = await BuildReport(query);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("قراردادهای دوره");
        worksheet.RightToLeft = true;
        worksheet.Style.Alignment.ReadingOrder = XLAlignmentReadingOrderValues.RightToLeft;

        var headers = new[]
        {
            "شناسه",
            "دوره",
            "سازمان طرف قرارداد",
            "شماره قرارداد",
            "مبلغ توافقی",
            "شروع قرارداد",
            "پایان قرارداد",
            "وضعیت قرارداد",
            "وضعیت ارتباط",
            "توضیحات"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var headerRange = worksheet.Range(
            1, 1, 1, headers.Length);

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var row = index + 2;

            worksheet.Cell(row, 1).Value = item.Id;
            worksheet.Cell(row, 2).Value = item.CourseTitle;
            worksheet.Cell(row, 3).Value =
                item.PartnerOrganizationName;
            worksheet.Cell(row, 4).Value =
                item.ContractNumber ?? "";
            worksheet.Cell(row, 5).Value =
                item.AgreedPrice.HasValue
                    ? item.AgreedPrice.Value
                    : 0;

            worksheet.Cell(row, 6).Value =
                item.StartDate?.ToString("yyyy/MM/dd") ?? "";

            worksheet.Cell(row, 7).Value =
                item.EndDate?.ToString("yyyy/MM/dd") ?? "";

            worksheet.Cell(row, 8).Value =
                item.ContractStatus;

            worksheet.Cell(row, 9).Value =
                item.IsActive ? "فعال" : "غیرفعال";

            worksheet.Cell(row, 10).Value =
                item.Description ?? "";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "course-partner-organization-report.xlsx");
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> ExportPdf(
        [FromQuery] CoursePartnerOrganizationReportQueryDto query)
    {
        var items = await BuildReport(query);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                page.DefaultTextStyle(
                    x => x
                        .FontFamily("Noto Sans Arabic")
                        .FontSize(8));
                page.ContentFromRightToLeft();

                page.Header()
                    .Text("گزارش تفصیلی قراردادهای دوره با سازمان‌های طرف قرارداد")
                    .Bold()
                    .FontFamily("Noto Sans Arabic")
                    .FontSize(14);

                page.Content()
                    .PaddingTop(15)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25);
                            columns.RelativeColumn(2.1f);
                            columns.RelativeColumn(1.8f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.1f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(0.9f);
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                                .Element(HeaderCell)
                                .Text("شناسه");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("دوره");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("سازمان");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("شماره قرارداد");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("مبلغ توافقی");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("شروع");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("پایان");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("وضعیت قرارداد");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("وضعیت");
                        });

                        foreach (var item in items)
                        {
                            table.Cell()
                                .Element(BodyCell)
                                .Text(item.Id.ToString());

                            table.Cell()
                                .Element(BodyCell)
                                .Text(item.CourseTitle);

                            table.Cell()
                                .Element(BodyCell)
                                .Text(item.PartnerOrganizationName);

                            table.Cell()
                                .Element(BodyCell)
                                .Text(item.ContractNumber ?? "-");

                            table.Cell()
                                .Element(BodyCell)
                                .Text(
                                    item.AgreedPrice.HasValue
                                        ? item.AgreedPrice.Value.ToString("N0")
                                        : "-");

                            table.Cell()
                                .Element(BodyCell)
                                .Text(
                                    item.StartDate?
                                        .ToString("yyyy/MM/dd") ?? "-");

                            table.Cell()
                                .Element(BodyCell)
                                .Text(
                                    item.EndDate?
                                        .ToString("yyyy/MM/dd") ?? "-");

                            table.Cell()
                                .Element(BodyCell)
                                .Text(item.ContractStatus);

                            table.Cell()
                                .Element(BodyCell)
                                .Text(
                                    item.IsActive
                                        ? "فعال"
                                        : "غیرفعال");
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("تعداد قراردادها: ");
                        text.Span(items.Count.ToString());
                    });
            });
        });

        var pdfBytes = document.GeneratePdf();

        return File(
            pdfBytes,
            "application/pdf",
            "course-partner-organization-report.pdf");
    }

    private async Task<List<CoursePartnerOrganizationReportItemDto>>
        BuildReport(
            CoursePartnerOrganizationReportQueryDto query)
    {
        var itemsQuery = _context.CoursePartnerOrganizations
            .AsNoTracking()
            .Include(x => x.Course)
            .Include(x => x.PartnerOrganization)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            itemsQuery = itemsQuery.Where(x =>
                (x.Course != null &&
                 x.Course.Title.Contains(search)) ||
                (x.PartnerOrganization != null &&
                 x.PartnerOrganization.Name.Contains(search)) ||
                (x.ContractNumber != null &&
                 x.ContractNumber.Contains(search)) ||
                (x.Description != null &&
                 x.Description.Contains(search)));
        }

        if (query.CourseId.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.CourseId == query.CourseId.Value);
        }

        if (query.PartnerOrganizationId.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.PartnerOrganizationId ==
                query.PartnerOrganizationId.Value);
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
            var endExclusive =
                query.StartTo.Value.Date.AddDays(1);

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
            var endExclusive =
                query.EndTo.Value.Date.AddDays(1);

            itemsQuery = itemsQuery.Where(x =>
                x.EndDate.HasValue &&
                x.EndDate.Value < endExclusive);
        }

        if (query.MinAgreedPrice.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.AgreedPrice.HasValue &&
                x.AgreedPrice.Value >=
                query.MinAgreedPrice.Value);
        }

        if (query.MaxAgreedPrice.HasValue)
        {
            itemsQuery = itemsQuery.Where(x =>
                x.AgreedPrice.HasValue &&
                x.AgreedPrice.Value <=
                query.MaxAgreedPrice.Value);
        }

        var items = await itemsQuery
            .Select(x => new CoursePartnerOrganizationReportItemDto
            {
                Id = x.Id,
                CourseId = x.CourseId,
                CourseTitle = x.Course != null
                    ? x.Course.Title
                    : string.Empty,

                PartnerOrganizationId =
                    x.PartnerOrganizationId,

                PartnerOrganizationName =
                    x.PartnerOrganization != null
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

        foreach (var item in items)
        {
            item.ContractStatus =
                GetContractStatus(
                    item.IsActive,
                    item.StartDate,
                    item.EndDate);
        }

        return query.SortBy?.ToLowerInvariant() switch
        {
            "coursename" or "coursetitle" =>
                query.SortDescending
                    ? items.OrderByDescending(x => x.CourseTitle).ToList()
                    : items.OrderBy(x => x.CourseTitle).ToList(),

            "partnerorganization" or
            "partnerorganizationname" =>
                query.SortDescending
                    ? items.OrderByDescending(
                        x => x.PartnerOrganizationName).ToList()
                    : items.OrderBy(
                        x => x.PartnerOrganizationName).ToList(),

            "contractnumber" =>
                query.SortDescending
                    ? items.OrderByDescending(
                        x => x.ContractNumber).ToList()
                    : items.OrderBy(
                        x => x.ContractNumber).ToList(),

            "agreedprice" =>
                query.SortDescending
                    ? items.OrderByDescending(
                        x => x.AgreedPrice).ToList()
                    : items.OrderBy(
                        x => x.AgreedPrice).ToList(),

            "startdate" =>
                query.SortDescending
                    ? items.OrderByDescending(
                        x => x.StartDate).ToList()
                    : items.OrderBy(
                        x => x.StartDate).ToList(),

            "enddate" =>
                query.SortDescending
                    ? items.OrderByDescending(
                        x => x.EndDate).ToList()
                    : items.OrderBy(
                        x => x.EndDate).ToList(),

            "status" or "contractstatus" =>
                query.SortDescending
                    ? items.OrderByDescending(
                        x => x.ContractStatus).ToList()
                    : items.OrderBy(
                        x => x.ContractStatus).ToList(),

            _ =>
                query.SortDescending
                    ? items.OrderByDescending(
                        x => x.Id).ToList()
                    : items.OrderBy(
                        x => x.Id).ToList()
        };
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
            return "آتی";

        if (endDate.HasValue &&
            endDate.Value.Date < today)
            return "منقضی";

        return "فعال";
    }

    private static IContainer HeaderCell(
        IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten2)
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .Padding(4)
            .AlignCenter();
    }

    private static IContainer BodyCell(
        IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(4);
    }
}
