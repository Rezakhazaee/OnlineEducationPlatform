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
public class CourseReportExportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CourseReportExportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("excel")]
    public async Task<IActionResult> Excel(
        [FromQuery] CourseReportQueryDto query)
    {
        var courses = await BuildQuery(query)
            .Select(c => new CourseReportItemDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                DeliveryType = c.DeliveryType,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor != null
                    ? c.Instructor.FullName
                    : null,
                IsActive = c.IsActive
            })
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("گزارش دوره‌ها");

        sheet.RightToLeft = true;

        sheet.Cell(1, 1).Value = "گزارش دوره‌ها";

        string[] headers =
        {
            "ردیف",
            "عنوان دوره",
            "مدرس",
            "نوع برگزاری",
            "قیمت",
            "وضعیت"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(3, i + 1).Value = headers[i];
            sheet.Cell(3, i + 1).Style.Font.Bold = true;
        }

        for (var i = 0; i < courses.Count; i++)
        {
            var row = i + 4;
            var course = courses[i];

            sheet.Cell(row, 1).Value = i + 1;
            sheet.Cell(row, 2).Value = course.Title;
            sheet.Cell(row, 3).Value =
                course.InstructorName ?? "بدون مدرس";

            sheet.Cell(row, 4).Value =
                course.DeliveryType switch
                {
                    "Online" => "آنلاین",
                    "InPerson" => "حضوری",
                    "Hybrid" => "ترکیبی",
                    _ => course.DeliveryType
                };

            sheet.Cell(row, 5).Value = course.Price;
            sheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";

            sheet.Cell(row, 6).Value =
                course.IsActive ? "فعال" : "غیرفعال";
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "course-report.xlsx");
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> Pdf(
        [FromQuery] CourseReportQueryDto query)
    {
        var courses = await BuildQuery(query)
            .Select(c => new CourseReportItemDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                DeliveryType = c.DeliveryType,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor != null
                    ? c.Instructor.FullName
                    : null,
                IsActive = c.IsActive
            })
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);
                page.ContentFromRightToLeft();

                page.DefaultTextStyle(style =>
                    style
                        .FontFamily("Noto Sans Arabic")
                        .FontSize(8));

                page.Header()
                    .AlignCenter()
                    .Text("گزارش دوره‌ها")
                    .Bold()
                    .FontSize(16);

                page.Content()
                    .PaddingTop(15)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.2f);
                            columns.ConstantColumn(90);
                            columns.ConstantColumn(65);
                        });

                        HeaderCell(table.Cell(), "ردیف");
                        HeaderCell(table.Cell(), "عنوان دوره");
                        HeaderCell(table.Cell(), "مدرس");
                        HeaderCell(table.Cell(), "نوع برگزاری");
                        HeaderCell(table.Cell(), "قیمت");
                        HeaderCell(table.Cell(), "وضعیت");

                        foreach (var course in courses)
                        {
                            BodyCell(table.Cell(), course.Id.ToString());
                            BodyCell(table.Cell(), course.Title);
                            BodyCell(
                                table.Cell(),
                                course.InstructorName ?? "بدون مدرس");

                            BodyCell(
                                table.Cell(),
                                course.DeliveryType switch
                                {
                                    "Online" => "آنلاین",
                                    "InPerson" => "حضوری",
                                    "Hybrid" => "ترکیبی",
                                    _ => course.DeliveryType
                                });

                            BodyCell(
                                table.Cell(),
                                course.Price.ToString("N0"));

                            BodyCell(
                                table.Cell(),
                                course.IsActive
                                    ? "فعال"
                                    : "غیرفعال");
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("آموزش‌یار - گزارش دوره‌ها");
                    });
            });
        });

        var bytes = document.GeneratePdf();

        return File(
            bytes,
            "application/pdf",
            "course-report.pdf");
    }

    private IQueryable<BackEnd.Models.Course> BuildQuery(
        CourseReportQueryDto query)
    {
        var courses = _context.Courses
            .AsNoTracking()
            .Include(c => c.Instructor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            courses = courses.Where(c =>
                c.Title.Contains(search) ||
                (c.Description != null &&
                 c.Description.Contains(search)));
        }

        if (query.InstructorId.HasValue)
        {
            courses = courses.Where(
                c => c.InstructorId == query.InstructorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.DeliveryType))
        {
            courses = courses.Where(
                c => c.DeliveryType == query.DeliveryType);
        }

        if (query.IsActive.HasValue)
        {
            courses = courses.Where(
                c => c.IsActive == query.IsActive.Value);
        }

        if (query.MinPrice.HasValue)
        {
            courses = courses.Where(
                c => c.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            courses = courses.Where(
                c => c.Price <= query.MaxPrice.Value);
        }

        return query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "price" => query.SortDescending
                ? courses.OrderByDescending(c => c.Price)
                : courses.OrderBy(c => c.Price),

            "instructor" => query.SortDescending
                ? courses.OrderByDescending(
                    c => c.Instructor!.FullName)
                : courses.OrderBy(
                    c => c.Instructor!.FullName),

            "deliverytype" => query.SortDescending
                ? courses.OrderByDescending(c => c.DeliveryType)
                : courses.OrderBy(c => c.DeliveryType),

            "status" => query.SortDescending
                ? courses.OrderByDescending(c => c.IsActive)
                : courses.OrderBy(c => c.IsActive),

            _ => query.SortDescending
                ? courses.OrderByDescending(c => c.Title)
                : courses.OrderBy(c => c.Title)
        };
    }

    private static void HeaderCell(
        IContainer cell,
        string text)
    {
        cell
            .Background("#263d4a")
            .Border(1)
            .BorderColor("#cccccc")
            .Padding(5)
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
            .Padding(5)
            .AlignMiddle()
            .Text(text);
    }
}
