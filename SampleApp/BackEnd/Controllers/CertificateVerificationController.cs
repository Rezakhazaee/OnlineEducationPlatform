using BackEnd.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}
