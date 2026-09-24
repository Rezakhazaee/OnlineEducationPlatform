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
[Authorize(Roles = "Admin")]
public class UserReportExportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserReportExportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("excel")]
    public async Task<IActionResult> Excel(
        [FromQuery] UserReportQueryDto query)
    {
        var users = await BuildQuery(query).ToListAsync();

        using var workbook = new XLWorkbook();

        var sheet = workbook.Worksheets.Add("گزارش کاربران");
        sheet.RightToLeft = true;

        sheet.Cell(1, 1).Value = "گزارش کاربران";
        sheet.Cell(1, 1).Style.Font.Bold = true;

        string[] headers =
        {
            "ردیف",
            "نام و نام خانوادگی",
            "موبایل",
            "نام کاربری",
            "نقش",
            "وضعیت",
            "تاریخ ایجاد"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(3, i + 1).Value = headers[i];
            sheet.Cell(3, i + 1).Style.Font.Bold = true;
        }

        for (var i = 0; i < users.Count; i++)
        {
            var row = i + 4;
            var user = users[i];

            sheet.Cell(row, 1).Value = i + 1;
            sheet.Cell(row, 2).Value = user.FullName;
            sheet.Cell(row, 3).Value = user.Mobile;
            sheet.Cell(row, 4).Value = user.Username;
            sheet.Cell(row, 5).Value = GetRoleName(user.Role);
            sheet.Cell(row, 6).Value =
                user.IsActive ? "فعال" : "غیرفعال";
            sheet.Cell(row, 7).Value =
                user.CreatedAt.ToString("yyyy/MM/dd");
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "user-report.xlsx");
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> Pdf(
        [FromQuery] UserReportQueryDto query)
    {
        var users = await BuildQuery(query).ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);
                page.ContentFromRightToLeft();

                page.DefaultTextStyle(style =>
                    style.FontFamily("Noto Sans Arabic")
                        .FontSize(8));

                page.Header()
                    .AlignCenter()
                    .Text("گزارش کاربران")
                    .Bold()
                    .FontSize(16);

                page.Content()
                    .PaddingTop(12)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.2f);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(75);
                        });

                        foreach (var header in new[]
                        {
                            "ردیف",
                            "نام و نام خانوادگی",
                            "موبایل",
                            "نام کاربری",
                            "نقش",
                            "وضعیت",
                            "تاریخ ایجاد"
                        })
                        {
                            HeaderCell(table.Cell(), header);
                        }

                        for (var i = 0; i < users.Count; i++)
                        {
                            var user = users[i];

                            BodyCell(
                                table.Cell(),
                                (i + 1).ToString());

                            BodyCell(
                                table.Cell(),
                                user.FullName);

                            BodyCell(
                                table.Cell(),
                                user.Mobile);

                            BodyCell(
                                table.Cell(),
                                user.Username);

                            BodyCell(
                                table.Cell(),
                                GetRoleName(user.Role));

                            BodyCell(
                                table.Cell(),
                                user.IsActive
                                    ? "فعال"
                                    : "غیرفعال");

                            BodyCell(
                                table.Cell(),
                                user.CreatedAt.ToString("yyyy/MM/dd"));
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("آموزش‌یار - گزارش کاربران");
            });
        });

        return File(
            document.GeneratePdf(),
            "application/pdf",
            "user-report.pdf");
    }

    private IQueryable<BackEnd.Models.User> BuildQuery(
        UserReportQueryDto query)
    {
        var users = _context.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            users = users.Where(u =>
                u.FullName.Contains(search) ||
                u.Mobile.Contains(search) ||
                u.Username.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
            users = users.Where(u => u.Role == query.Role);

        if (query.IsActive.HasValue)
            users = users.Where(
                u => u.IsActive == query.IsActive.Value);

        if (query.CreatedFrom.HasValue)
            users = users.Where(
                u => u.CreatedAt >= query.CreatedFrom.Value);

        if (query.CreatedTo.HasValue)
        {
            var endDate = query.CreatedTo.Value.Date.AddDays(1);

            users = users.Where(
                u => u.CreatedAt < endDate);
        }

        return query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "fullname" => query.SortDescending
                ? users.OrderByDescending(u => u.FullName)
                : users.OrderBy(u => u.FullName),

            "mobile" => query.SortDescending
                ? users.OrderByDescending(u => u.Mobile)
                : users.OrderBy(u => u.Mobile),

            "username" => query.SortDescending
                ? users.OrderByDescending(u => u.Username)
                : users.OrderBy(u => u.Username),

            "role" => query.SortDescending
                ? users.OrderByDescending(u => u.Role)
                : users.OrderBy(u => u.Role),

            "status" => query.SortDescending
                ? users.OrderByDescending(u => u.IsActive)
                : users.OrderBy(u => u.IsActive),

            _ => query.SortDescending
                ? users.OrderByDescending(u => u.CreatedAt)
                : users.OrderBy(u => u.CreatedAt)
        };
    }

    private static string GetRoleName(string role) =>
        role switch
        {
            "Admin" => "مدیر",
            "EducationStaff" => "کارشناس آموزش",
            "Marketer" => "بازاریاب",
            "Support" => "پشتیبان",
            "Instructor" => "مدرس",
            "Student" => "دانشجو",
            _ => role
        };

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
