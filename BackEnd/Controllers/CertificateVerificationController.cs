using BackEnd.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/CertificateVerification")]
public class CertificateVerificationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CertificateVerificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{certificateNumber}")]
    public async Task<IActionResult> Verify(string certificateNumber)
    {
        var certificate = await _context.Certificates
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.CertificateNumber == certificateNumber);

        if (certificate == null)
        {
            return NotFound(new
            {
                message = "گواهی مورد نظر پیدا نشد."
            });
        }

        return Ok(new
        {
            certificate.CertificateNumber,
            certificate.StudentName,
            certificate.CourseTitle,
            certificate.IssuedAtUtc
        });
    }

    [HttpGet("{certificateNumber}/pdf")]
    public async Task<IActionResult> Pdf(string certificateNumber)
    {
        var certificate = await _context.Certificates
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.CertificateNumber == certificateNumber);

        if (certificate == null)
        {
            return NotFound(new
            {
                message = "گواهی مورد نظر پیدا نشد."
            });
        }

        var pdf = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.PageColor(Colors.White);
                page.ContentFromRightToLeft();

                page.DefaultTextStyle(style =>
                    style
                        .FontFamily("Noto Kufi Arabic")
                        .FontSize(15)
                        .FontColor(Colors.Grey.Darken3));

                page.Content()
                    .AlignCenter()
                    .Column(column =>
                    {
                        column.Spacing(22);

                        column.Item()
                            .PaddingTop(45)
                            .AlignCenter()
                            .Text("آموزش‌یار")
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item()
                            .AlignCenter()
                            .Text("گواهی پایان دوره")
                            .FontSize(30)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);

                        column.Item()
                            .PaddingVertical(20)
                            .Border(2)
                            .BorderColor(Colors.Blue.Medium)
                            .Padding(30)
                            .Column(card =>
                            {
                                card.Spacing(18);

                                card.Item()
                                    .AlignCenter()
                                    .Text("بدین‌وسیله گواهی می‌شود");

                                card.Item()
                                    .AlignCenter()
                                    .Text(certificate.StudentName)
                                    .FontSize(22)
                                    .Bold();

                                card.Item()
                                    .AlignCenter()
                                    .Text("دوره زیر را با موفقیت به پایان رسانده است");

                                card.Item()
                                    .AlignCenter()
                                    .Text(certificate.CourseTitle)
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            });

                        column.Item()
                            .PaddingTop(20)
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("شماره گواهی: ").Bold();
                                text.Span(certificate.CertificateNumber);
                            });

                        column.Item()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("تاریخ صدور: ").Bold();
                                text.Span(
                                    certificate.IssuedAtUtc
                                        .ToLocalTime()
                                        .ToString("yyyy/MM/dd"));
                            });

                        column.Item()
                            .PaddingTop(55)
                            .AlignCenter()
                            .Text("این گواهی توسط سامانه آموزش‌یار صادر شده است.")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken1);
                    });
            });
        }).GeneratePdf();

        return File(
            pdf,
            "application/pdf",
            $"{certificate.CertificateNumber}.pdf");
    }
}
