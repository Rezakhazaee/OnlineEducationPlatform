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
    [Authorize(Roles = "Admin,Support")]
    [HttpGet]
    public async Task<List<PaymentDetailDto>> Get([FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] string? paymentType = null, [FromQuery] decimal? minAmount = null)
    {
        var query = _context.Payments.AsQueryable();

        if (User.IsInRole("Support"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var supportUserId))
            {
                return new List<PaymentDetailDto>();
            }

            query = query.Where(p =>
                p.Enrollment != null &&
                p.Enrollment.Student != null &&
                p.Enrollment.Student.SupportUserId == supportUserId);
        }

        if (!string.IsNullOrWhiteSpace(search)) {
            var term = search.Trim();
            var hasId = int.TryParse(term, out var paymentId);
            query = query.Where(p =>
                (p.Enrollment != null && p.Enrollment.Student != null &&
                 (p.Enrollment.Student.FirstName + " " + p.Enrollment.Student.LastName).Contains(term)) ||
                (p.Enrollment != null && p.Enrollment.Course != null && p.Enrollment.Course.Title.Contains(term)) ||
                (hasId && p.Id == paymentId));
        }
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(paymentType)) query = query.Where(p => p.PaymentType == paymentType);
        if (minAmount.HasValue) query = query.Where(p => p.Amount >= minAmount.Value);

        return await query
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
    [Authorize(Roles = "Admin,Support")]  
    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto dto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        // کنترل تاریخ پرداخت: پرداخت آینده مجاز نیست
        if (dto.PaymentDate > DateTime.Now)
        {
            return BadRequest(new { message = "تاریخ پرداخت نمی‌تواند در آینده باشد" });
        }

        // ----------------------------------------
        // 1. بررسی وضعیت پرداخت
        // ----------------------------------------

        var validStatuses = new[]
        {
            "Paid",
            "Pending",
            "Cancelled"
        };

        var normalizedStatus = validStatuses.FirstOrDefault(
            s => string.Equals(s, dto.Status?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (normalizedStatus == null)
        {
            return BadRequest(new
            {
                message = "وضعیت پرداخت نامعتبر است",
                allowedStatuses = validStatuses
            });
        }

        dto.Status = normalizedStatus;

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

        var normalizedPaymentType = validPaymentTypes.FirstOrDefault(
            s => string.Equals(s, dto.PaymentType?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (normalizedPaymentType == null)
        {
            return BadRequest(new
            {
                message = "نوع پرداخت نامعتبر است",
                allowedPaymentTypes = validPaymentTypes
            });
        }

        dto.PaymentType = normalizedPaymentType;

        // ----------------------------------------
        // 3. بررسی وجود Enrollment
        // ----------------------------------------

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.CoursePartnerOrganization)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

        if (enrollment == null)
        {
            return BadRequest(new
            {
                message = "ثبت نام مورد نظر وجود ندارد"
            });
        }

        if (User.IsInRole("Support"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var supportUserId) ||
                enrollment.Student?.SupportUserId != supportUserId)
            {
                return NotFound(new
                {
                    message = "ثبت نام مورد نظر وجود ندارد"
                });
            }
        }

        // ----------------------------------------
// 3.5 بررسی وضعیت ثبت نام
// ----------------------------------------

if (enrollment.Status == "Cancelled" ||
    enrollment.Status == "Suspended")
{
    return BadRequest(new
    {
        message = "برای ثبت نام لغوشده یا تعلیق‌شده، امکان ثبت پرداخت وجود ندارد",
        enrollmentStatus = enrollment.Status
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


        var coursePrice = enrollment.CoursePartnerOrganization?.AgreedPrice ?? enrollment.Course.Price;

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

        int? installmentId = null;

        if (dto.PaymentType != "FullPayment")
        {
            var installmentNumber = dto.PaymentType switch
            {
                "FirstInstallment" => 1,
                "SecondInstallment" => 2,
                "ThirdInstallment" => 3,
                _ => 0
            };
            var installment = await _context.Installments.FirstOrDefaultAsync(i => i.EnrollmentId == enrollment.Id && i.InstallmentNumber == installmentNumber);
            if (installment == null) return BadRequest(new { message = "قسط مربوط به این ثبت‌نام پیدا نشد" });
            installmentId = installment.Id;

            if (dto.Status == "Paid")
            {
                var installmentPaid = await _context.Payments
                    .Where(p => p.InstallmentId == installment.Id && p.Status == "Paid")
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                if (dto.Amount + installmentPaid > installment.Amount)
                {
                    return BadRequest(new { message = "مبلغ پرداخت از مانده قسط بیشتر است" });
                }
            }
        }

        var payment = new Payment
        {
            EnrollmentId = dto.EnrollmentId,

            InstallmentId = installmentId,

            Amount = dto.Amount,

            PaymentDate = dto.PaymentDate,

            PaymentType = dto.PaymentType,

            Description = dto.Description,

            Status = dto.Status
        };


        _context.Payments.Add(payment);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();


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
[Authorize(Roles = "Admin,Support")]
[HttpPut("{id}/pay")]
public async Task<IActionResult> Pay(int id)
{
    await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
    var payment = await _context.Payments
        .Include(p => p.Enrollment)
        .ThenInclude(e => e!.Course)
        .Include(p => p.Enrollment)
        .ThenInclude(e => e!.CoursePartnerOrganization)
                .Include(p => p.Enrollment)
        .ThenInclude(e => e!.Student)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (payment == null)
    {
        return NotFound(new
        {
            message = "پرداخت مورد نظر پیدا نشد"
        });
    }

    if (payment.Enrollment == null)
    {
        return BadRequest(new
        {
            message = "ثبت نام مربوط به این پرداخت پیدا نشد"
        });
    }


    if (User.IsInRole("Support"))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var supportUserId) ||
            payment.Enrollment.Student?.SupportUserId != supportUserId)
        {
            return NotFound(new
            {
                message = "پرداخت مورد نظر پیدا نشد"
            });
        }
    }

    if (payment.Status != "Pending")
    {
        return BadRequest(new
        {
            message = "فقط پرداخت‌های Pending قابل تأیید هستند",
            currentStatus = payment.Status
        });
    }

if (payment.Enrollment.Status == "Cancelled" ||
    payment.Enrollment.Status == "Suspended")
{
    return BadRequest(new
    {
        message = "برای ثبت نام لغوشده یا تعلیق‌شده، امکان تأیید پرداخت وجود ندارد",
        enrollmentStatus = payment.Enrollment.Status
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

    var coursePrice = payment.Enrollment.CoursePartnerOrganization?.AgreedPrice ?? payment.Enrollment.Course.Price;

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
    if (payment.PaymentType == "FullPayment")
    {
        var remainingAmount = Math.Max(coursePrice - totalPaid, 0);

        if (payment.Amount != remainingAmount)
        {
            return BadRequest(new
            {
                message = "مبلغ پرداخت کامل باید دقیقاً برابر مبلغ باقی‌مانده باشد",
                coursePrice,
                totalPaid,
                remainingAmount,
                paymentAmount = payment.Amount
            });
        }
    }

    // کنترل تاریخ پرداخت پیش از تأیید
    if (payment.PaymentDate > DateTime.Now)
    {
        return BadRequest(new
        {
            message = "تاریخ پرداخت نمی‌تواند در آینده باشد"
        });
    }

    // اتصال پرداخت قسطی قدیمی به رکورد قسط
    if (payment.PaymentType != "FullPayment" && !payment.InstallmentId.HasValue)
    {
        var installmentNumber = payment.PaymentType switch
        {
            "FirstInstallment" => 1,
            "SecondInstallment" => 2,
            "ThirdInstallment" => 3,
            _ => 0
        };

        var installment = await _context.Installments.FirstOrDefaultAsync(i =>
            i.EnrollmentId == payment.EnrollmentId &&
            i.InstallmentNumber == installmentNumber);

        if (installment == null)
        {
            return BadRequest(new { message = "قسط مربوط به این پرداخت پیدا نشد" });
        }

        payment.InstallmentId = installment.Id;
    }

    // کنترل سقف مبلغ قسط هنگام تأیید
    if (payment.PaymentType != "FullPayment" && payment.InstallmentId.HasValue)
    {
        var installment = await _context.Installments.FirstOrDefaultAsync(i =>
            i.Id == payment.InstallmentId.Value &&
            i.EnrollmentId == payment.EnrollmentId);

        if (installment == null)
            return BadRequest(new { message = "قسط مربوط به این پرداخت پیدا نشد" });

        var expectedInstallmentNumber = payment.PaymentType switch
        {
            "FirstInstallment" => 1,
            "SecondInstallment" => 2,
            "ThirdInstallment" => 3,
            _ => 0
        };

        if (installment.InstallmentNumber != expectedInstallmentNumber)
            return BadRequest(new { message = "نوع پرداخت با قسط انتخاب‌شده سازگار نیست" });

        var installmentPaid = await _context.Payments
            .Where(p => p.InstallmentId == installment.Id &&
                        p.Id != payment.Id && p.Status == "Paid")
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        if (installmentPaid + payment.Amount > installment.Amount)
            return BadRequest(new { message = "مبلغ پرداخت از مانده قسط بیشتر است" });
    }

    // تغییر وضعیت
    payment.Status = "Paid";

    await _context.SaveChangesAsync();
    await transaction.CommitAsync();

    return Ok(new
    {
        message = "پرداخت با موفقیت تأیید شد",
        paymentId = payment.Id,
        status = payment.Status
    });
}

// تغییر وضعیت پرداخت از Pending به Cancelled
[Authorize(Roles = "Admin,Support")]
[HttpPut("{id}/cancel")]
public async Task<IActionResult> Cancel(int id)
{
    var payment = await _context.Payments
        .Include(p => p.Enrollment)
        .ThenInclude(e => e!.Student)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (payment == null)
    {
        return NotFound(new
        {
            message = "پرداخت مورد نظر پیدا نشد"
        });
    }

    
    if (User.IsInRole("Support"))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var supportUserId) ||
            payment.Enrollment?.Student?.SupportUserId != supportUserId)
        {
            return NotFound(new
            {
                message = "پرداخت مورد نظر پیدا نشد"
            });
        }
    }

    var affectedRows = await _context.Payments
        .Where(p => p.Id == id && p.Status == "Pending")
        .ExecuteUpdateAsync(update => update.SetProperty(p => p.Status, "Cancelled"));

    if (affectedRows == 0)
    {
        var currentStatus = await _context.Payments
            .Where(p => p.Id == id)
            .Select(p => p.Status)
            .FirstOrDefaultAsync();

        if (currentStatus == null)
        {
            return NotFound(new { message = "پرداخت مورد نظر پیدا نشد" });
        }

        return Conflict(new
        {
            message = "وضعیت پرداخت تغییر کرده است؛ صفحه را تازه‌سازی کنید",
            currentStatus
        });
    }

    payment.Status = "Cancelled";

    return Ok(new
    {
        message = "پرداخت با موفقیت لغو شد",
        paymentId = payment.Id,
        status = payment.Status
    });
}
}