using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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

    
    
    // Student - مشاهده پرداخت‌های خودش
    [Authorize(Roles = "Student")]
    [HttpGet("my")]
    public async Task<ActionResult<List<PaymentDetailDto>>> GetMyPayments()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
        {
            return NotFound(new
            {
                message = "پروفایل دانشجویی برای این کاربر پیدا نشد"
            });
        }

        var payments = await _context.Payments
            .Where(p =>
                p.Enrollment != null &&
                p.Enrollment.StudentId == student.Id)
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

        return Ok(payments);
    }

    
    // ثبت پرداخت جدید
    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto dto)
    {
        // ----------------------------------------
        // 1. بررسی وضعیت پرداخت
        // ----------------------------------------

        var validStatuses = new[]
        {
            "Paid",
            "Pending",
            "Cancelled"
        };

        if (!validStatuses.Contains(dto.Status))
        {
            return BadRequest(new
            {
                message = "وضعیت پرداخت نامعتبر است",
                allowedStatuses = validStatuses
            });
        }


        // ----------------------------------------
        // 2. بررسی نوع پرداخت
        // ----------------------------------------

        var validPaymentTypes = new[]
        {
            "FirstInstallment",
            "SecondInstallment",
            "ThirdInstallment",
            "FullPayment"
        };

        if (!validPaymentTypes.Contains(dto.PaymentType))
        {
            return BadRequest(new
            {
                message = "نوع پرداخت نامعتبر است",
                allowedPaymentTypes = validPaymentTypes
            });
        }


        // ----------------------------------------
        // 3. بررسی وجود Enrollment
        // ----------------------------------------

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

        if (enrollment == null)
        {
            return BadRequest(new
            {
                message = "ثبت نام مورد نظر وجود ندارد"
            });
        }


        // ----------------------------------------
        // 4. بررسی وجود Course
        // ----------------------------------------

        if (enrollment.Course == null)
        {
            return BadRequest(new
            {
                message = "دوره مربوط به این ثبت نام وجود ندارد"
            });
        }


        // ----------------------------------------
        // 5. بررسی مبلغ
        // ----------------------------------------

        if (dto.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "مبلغ پرداخت باید بیشتر از صفر باشد"
            });
        }


        // ----------------------------------------
        // 6. جلوگیری از ثبت قسط تکراری
        // ----------------------------------------

        if (dto.Status == "Paid" &&
            dto.PaymentType != "FullPayment")
        {
            var installmentAlreadyExists = await _context.Payments
                .AnyAsync(p =>
                    p.EnrollmentId == dto.EnrollmentId &&
                    p.PaymentType == dto.PaymentType &&
                    p.Status == "Paid");

            if (installmentAlreadyExists)
            {
                return BadRequest(new
                {
                    message = "این قسط قبلاً ثبت شده است",
                    enrollmentId = dto.EnrollmentId,
                    paymentType = dto.PaymentType
                });
            }
        }


        // ----------------------------------------
        // 7. کنترل ترتیب اقساط
        // ----------------------------------------

        if (dto.Status == "Paid")
        {
            if (dto.PaymentType == "SecondInstallment")
            {
                var firstInstallmentExists = await _context.Payments
                    .AnyAsync(p =>
                        p.EnrollmentId == dto.EnrollmentId &&
                        p.PaymentType == "FirstInstallment" &&
                        p.Status == "Paid");

                if (!firstInstallmentExists)
                {
                    return BadRequest(new
                    {
                        message = "ابتدا باید قسط اول پرداخت شود"
                    });
                }
            }


            if (dto.PaymentType == "ThirdInstallment")
            {
                var secondInstallmentExists = await _context.Payments
                    .AnyAsync(p =>
                        p.EnrollmentId == dto.EnrollmentId &&
                        p.PaymentType == "SecondInstallment" &&
                        p.Status == "Paid");

                if (!secondInstallmentExists)
                {
                    return BadRequest(new
                    {
                        message = "ابتدا باید قسط دوم پرداخت شود"
                    });
                }
            }
        }


        // ----------------------------------------
        // 8. محاسبه مجموع پرداخت‌های موفق
        // ----------------------------------------

        var totalPaid = await _context.Payments
            .Where(p =>
                p.EnrollmentId == dto.EnrollmentId &&
                p.Status == "Paid")
            .SumAsync(p => (decimal?)p.Amount) ?? 0;


        var coursePrice = enrollment.Course.Price;

        var totalAfterPayment = totalPaid + dto.Amount;


        // ----------------------------------------
        // 9. جلوگیری از پرداخت بیشتر از قیمت دوره
        // ----------------------------------------

        if (dto.Status == "Paid" &&
            totalAfterPayment > coursePrice)
        {
            var remainingAmount = Math.Max(
                coursePrice - totalPaid,
                0);

            return BadRequest(new
            {
                message = "مجموع پرداخت‌ها نمی‌تواند بیشتر از قیمت دوره باشد",

                coursePrice = coursePrice,

                totalPaid = totalPaid,

                newPaymentAmount = dto.Amount,

                totalAfterPayment = totalAfterPayment,

                remainingAmount = remainingAmount
            });
        }


        // ----------------------------------------
        // 10. FullPayment باید مبلغ باقی‌مانده باشد
        // ----------------------------------------

        if (dto.Status == "Paid" &&
            dto.PaymentType == "FullPayment")
        {
            var remainingAmount = Math.Max(
                coursePrice - totalPaid,
                0);

            if (dto.Amount != remainingAmount)
            {
                return BadRequest(new
                {
                    message = "مبلغ پرداخت کامل باید دقیقاً برابر مبلغ باقی‌مانده باشد",

                    coursePrice = coursePrice,

                    totalPaid = totalPaid,

                    remainingAmount = remainingAmount,

                    newPaymentAmount = dto.Amount
                });
            }
        }


        // ----------------------------------------
        // 11. ثبت Payment
        // ----------------------------------------

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


        // ----------------------------------------
        // 12. ساخت نتیجه
        // ----------------------------------------

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

    // تغییر وضعیت پرداخت از Pending به Paid
[HttpPut("{id}/pay")]
public async Task<IActionResult> Pay(int id)
{
    var payment = await _context.Payments
        .Include(p => p.Enrollment)
        .ThenInclude(e => e!.Course)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (payment == null)
    {
        return NotFound(new
        {
            message = "پرداخت مورد نظر پیدا نشد"
        });
    }

    if (payment.Status != "Pending")
    {
        return BadRequest(new
        {
            message = "فقط پرداخت‌های Pending قابل تأیید هستند",
            currentStatus = payment.Status
        });
    }

    if (payment.Enrollment == null)
    {
        return BadRequest(new
        {
            message = "ثبت نام مربوط به این پرداخت پیدا نشد"
        });
    }

    if (payment.Enrollment.Course == null)
    {
        return BadRequest(new
        {
            message = "دوره مربوط به این ثبت نام پیدا نشد"
        });
    }

    // جلوگیری از ثبت قسط تکراری
    if (payment.PaymentType != "FullPayment")
    {
        var installmentAlreadyExists = await _context.Payments
            .AnyAsync(p =>
                p.Id != payment.Id &&
                p.EnrollmentId == payment.EnrollmentId &&
                p.PaymentType == payment.PaymentType &&
                p.Status == "Paid");

        if (installmentAlreadyExists)
        {
            return BadRequest(new
            {
                message = "این قسط قبلاً پرداخت شده است"
            });
        }
    }

    // کنترل ترتیب اقساط
    if (payment.PaymentType == "SecondInstallment")
    {
        var firstInstallmentExists = await _context.Payments
            .AnyAsync(p =>
                p.EnrollmentId == payment.EnrollmentId &&
                p.PaymentType == "FirstInstallment" &&
                p.Status == "Paid");

        if (!firstInstallmentExists)
        {
            return BadRequest(new
            {
                message = "ابتدا باید قسط اول پرداخت شود"
            });
        }
    }

    if (payment.PaymentType == "ThirdInstallment")
    {
        var secondInstallmentExists = await _context.Payments
            .AnyAsync(p =>
                p.EnrollmentId == payment.EnrollmentId &&
                p.PaymentType == "SecondInstallment" &&
                p.Status == "Paid");

        if (!secondInstallmentExists)
        {
            return BadRequest(new
            {
                message = "ابتدا باید قسط دوم پرداخت شود"
            });
        }
    }

    // محاسبه مجموع پرداخت‌های قبلی
    var totalPaid = await _context.Payments
        .Where(p =>
            p.EnrollmentId == payment.EnrollmentId &&
            p.Status == "Paid")
        .SumAsync(p => (decimal?)p.Amount) ?? 0;

    var coursePrice = payment.Enrollment.Course.Price;

    var totalAfterPayment = totalPaid + payment.Amount;

    // جلوگیری از پرداخت بیشتر از قیمت دوره
    if (totalAfterPayment > coursePrice)
    {
        return BadRequest(new
        {
            message = "مجموع پرداخت‌ها نمی‌تواند بیشتر از قیمت دوره باشد",
            coursePrice,
            totalPaid,
            paymentAmount = payment.Amount,
            totalAfterPayment
        });
    }
    // تغییر وضعیت
    payment.Status = "Paid";

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "پرداخت با موفقیت تأیید شد",
        paymentId = payment.Id,
        status = payment.Status
    });
}

// تغییر وضعیت پرداخت از Pending به Cancelled
[HttpPut("{id}/cancel")]
public async Task<IActionResult> Cancel(int id)
{
    var payment = await _context.Payments
        .FirstOrDefaultAsync(p => p.Id == id);

    if (payment == null)
    {
        return NotFound(new
        {
            message = "پرداخت مورد نظر پیدا نشد"
        });
    }

    if (payment.Status != "Pending")
    {
        return BadRequest(new
        {
            message = "فقط پرداخت‌های Pending قابل لغو هستند",
            currentStatus = payment.Status
        });
    }

    payment.Status = "Cancelled";

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "پرداخت با موفقیت لغو شد",
        paymentId = payment.Id,
        status = payment.Status
    });
}
}