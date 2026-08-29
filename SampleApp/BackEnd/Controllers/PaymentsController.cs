using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }


    // دریافت لیست پرداخت‌ها با اطلاعات دانشجو و دوره
    [HttpGet]
    public async Task<List<PaymentDetailDto>> Get()
    {
        return await _context.Payments
            .Select(p => new PaymentDetailDto
            {
                Id = p.Id,

                EnrollmentId = p.EnrollmentId,

                StudentName = p.Enrollment != null &&
                              p.Enrollment.Student != null
                    ? p.Enrollment.Student.FirstName + " " +
                      p.Enrollment.Student.LastName
                    : string.Empty,

                CourseTitle = p.Enrollment != null &&
                              p.Enrollment.Course != null
                    ? p.Enrollment.Course.Title
                    : string.Empty,

                Amount = p.Amount,

                PaymentDate = p.PaymentDate,

                PaymentType = p.PaymentType,

                Description = p.Description,

                Status = p.Status
            })
            .ToListAsync();
    }


    // ثبت پرداخت جدید
    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto dto)
    {
        // بررسی وجود ثبت نام
        var enrollmentExists = await _context.Enrollments
            .AnyAsync(e => e.Id == dto.EnrollmentId);

        if (!enrollmentExists)
        {
            return BadRequest(new
            {
                message = "ثبت نام مورد نظر وجود ندارد"
            });
        }


        // ایجاد پرداخت
        var payment = new Payment
        {
            EnrollmentId = dto.EnrollmentId,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate,
            PaymentType = dto.PaymentType,
            Description = dto.Description,
            Status = dto.Status
        };


        _context.Payments.Add(payment);

        await _context.SaveChangesAsync();


        // آماده سازی نتیجه
        var result = new PaymentDto
        {
            Id = payment.Id,
            EnrollmentId = payment.EnrollmentId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentType = payment.PaymentType,
            Description = payment.Description,
            Status = payment.Status
        };


        return result;
    }
}