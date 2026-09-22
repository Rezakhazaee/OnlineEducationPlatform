using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using BackEnd.Services;
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
    private readonly PackageAccessService _packageAccess;
    private readonly ZarinPalService _zarinPalService;
    private readonly IConfiguration _configuration;
    public PaymentsController(
        ApplicationDbContext context,
        PackageAccessService packageAccess,
        ZarinPalService zarinPalService,
        IConfiguration configuration)
    {
        _context = context;
        _packageAccess = packageAccess;
        _zarinPalService = zarinPalService;
        _configuration = configuration;
    }


    // دریافت لیست پرداخت‌ها با اطلاعات دانشجو و دوره

[Authorize(Roles = "Admin,EducationStaff,Marketer,Support,Instructor")]
[HttpGet]
public async Task<ActionResult<List<PaymentDetailDto>>> Get()
{
    var query = _context.Payments.AsQueryable();

    if (User.IsInRole("Support"))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        query = query.Where(p =>
            p.Enrollment != null &&
            p.Enrollment.Student != null &&
            p.Enrollment.Student.SupportUserId == userId);
    }

    if (User.IsInRole("Marketer"))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        query = query.Where(p =>
            p.Enrollment != null &&
            p.Enrollment.Student != null &&
            p.Enrollment.Student.MarketingUserId == userId);
    }

    if (User.IsInRole("Instructor"))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "شناسه کاربر معتبر نیست"
            });
        }

        query = query.Where(p =>
            p.Enrollment != null &&
            p.Enrollment.Course != null &&
            p.Enrollment.Course.InstructorId == userId);
    }

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

                  PaymentMethod = p.PaymentMethod,

                  GatewayRefId = p.GatewayRefId,
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
                  PaymentMethod = p.PaymentMethod,
                  GatewayRefId = p.GatewayRefId,
                PaymentDate = p.PaymentDate,
                PaymentType = p.PaymentType,
                Description = p.Description,
                Status = p.Status
            })
            .ToListAsync();

        return Ok(payments);
    }


    // ==========================================
    // Student - درخواست پرداخت از طریق درگاه
    // ==========================================

    [Authorize(Roles = "Student")]
    [HttpPost("my/gateway/request")]
    public async Task<IActionResult> CreateGatewayPayment(
        CreateGatewayPaymentDto dto)
    {
        if (!await _packageAccess.HasPackageAsync(3))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "درگاه پرداخت فقط در پکیج پرداخت آنلاین فعال است."
            });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "شناسه کاربر معتبر نیست" });
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
        {
            return NotFound(new { message = "پروفایل دانشجویی پیدا نشد" });
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.CoursePartnerOrganization)
            .FirstOrDefaultAsync(e =>
                e.Id == dto.EnrollmentId &&
                e.StudentId == student.Id);

        if (enrollment == null || enrollment.Course == null)
        {
            return NotFound(new { message = "ثبت نام مورد نظر پیدا نشد" });
        }

        if (enrollment.Status == "Cancelled" ||
            enrollment.Status == "Suspended")
        {
            return BadRequest(new
            {
                message = "برای این ثبت نام امکان پرداخت وجود ندارد"
            });
        }

        var allowedTypes = new[]
        {
            "FirstInstallment",
            "SecondInstallment",
            "ThirdInstallment",
            "FullPayment"
        };

        if (!allowedTypes.Contains(dto.PaymentType))
        {
            return BadRequest(new { message = "نوع پرداخت نامعتبر است" });
        }

        var coursePrice =
            enrollment.CoursePartnerOrganization?.AgreedPrice
            ?? enrollment.Course.Price;

        var totalPaid = await _context.Payments
            .Where(p =>
                p.EnrollmentId == enrollment.Id &&
                p.Status == "Paid")
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var remainingAmount = Math.Max(coursePrice - totalPaid, 0);

        if (remainingAmount <= 0)
        {
            return BadRequest(new
            {
                message = "این ثبت نام به طور کامل تسویه شده است"
            });
        }

        if (dto.Amount <= 0 || dto.Amount > remainingAmount)
        {
            return BadRequest(new
            {
                message = "مبلغ پرداخت نامعتبر است",
                remainingAmount
            });
        }

        if (dto.PaymentType == "FullPayment" &&
            dto.Amount != remainingAmount)
        {
            return BadRequest(new
            {
                message = "مبلغ پرداخت کامل باید برابر مبلغ باقی‌مانده باشد"
            });
        }

        var existingGatewayPayment = await _context.Payments
            .AnyAsync(p =>
                p.EnrollmentId == enrollment.Id &&
                p.PaymentMethod == "ZarinPal" &&
                p.PaymentType == dto.PaymentType &&
                (p.Status == "Pending" || p.Status == "Paid"));

        if (existingGatewayPayment)
        {
            return BadRequest(new
            {
                message = dto.PaymentType == "FullPayment"
                    ? "برای این ثبت‌نام قبلاً یک پرداخت کامل آنلاین ایجاد یا تأیید شده است."
                    : "برای این نوع پرداخت، یک تراکنش آنلاین قبلی هنوز معتبر است."
            });
        }

        var payment = new Payment
        {
            EnrollmentId = enrollment.Id,
            Amount = dto.Amount,
            PaymentDate = DateTime.Now,
            PaymentType = dto.PaymentType,
            Description = "پرداخت از طریق زرین‌پال",
            Status = "Pending",
            PaymentMethod = "ZarinPal"
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var callbackBaseUrl =
            _configuration["ZarinPal:CallbackBaseUrl"];

        if (string.IsNullOrWhiteSpace(callbackBaseUrl))
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return BadRequest(new
            {
                message = "CallbackBaseUrl زرین‌پال در تنظیمات سیستم وارد نشده است."
            });
        }

        var callbackUrl =
              callbackBaseUrl.TrimEnd('/') + 
            "/api/Payments/gateway/callback?paymentId=" +
            payment.Id;

        var gatewayResult =
            await _zarinPalService.RequestPaymentAsync(
                dto.Amount,
                callbackUrl,
                "پرداخت دوره - " + enrollment.Course.Title,
                student.Mobile);

        if (!gatewayResult.Success)
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return BadRequest(new
            {
                message = gatewayResult.Error ?? "خطا در ایجاد تراکنش"
            });
        }

        payment.GatewayAuthority = gatewayResult.Authority;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            paymentId = payment.Id,
            authority = gatewayResult.Authority,
            paymentUrl = gatewayResult.PaymentUrl
        });
    }

    // ==========================================
    // ZarinPal - Callback و تأیید پرداخت
    // ==========================================

    [AllowAnonymous]
    [HttpGet("gateway/callback")]
    public async Task<IActionResult> GatewayCallback(
        [FromQuery] int paymentId,
        [FromQuery] string? Authority,
        [FromQuery] string? Status)
    {
        var payment = await _context.Payments
            .Include(p => p.Enrollment)
            .ThenInclude(e => e!.Course)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            return NotFound(new
            {
                message = "پرداخت مورد نظر پیدا نشد"
            });
        }

        if (string.IsNullOrWhiteSpace(Authority) ||
            string.IsNullOrWhiteSpace(payment.GatewayAuthority) ||
            !string.Equals(
                payment.GatewayAuthority,
                Authority,
                StringComparison.Ordinal))
        {
            return BadRequest(new
            {
                message = "Authority تراکنش معتبر نیست"
            });
        }

        var frontendBaseUrl =
            _configuration["ZarinPal:FrontendBaseUrl"];

        string BuildRedirect(string result, string? refId = null, int? code = null)
        {
            if (string.IsNullOrWhiteSpace(frontendBaseUrl) ||
                payment.Enrollment == null)
            {
                return string.Empty;
            }

            var url =
                frontendBaseUrl.TrimEnd('/') +
                "/student/payment/" +
                payment.Enrollment.Id +
                "?gateway=" +
                Uri.EscapeDataString(result);

            if (!string.IsNullOrWhiteSpace(refId))
            {
                url +=
                    "&refId=" +
                    Uri.EscapeDataString(refId);
            }

            if (code.HasValue)
            {
                url += "&code=" + code.Value;
            }

            return url;
        }

        if (!string.Equals(
                Status,
                "OK",
                StringComparison.OrdinalIgnoreCase))
        {
            if (payment.Status == "Pending")
            {
                payment.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }

            var cancelledUrl = BuildRedirect("cancelled");

            if (!string.IsNullOrWhiteSpace(cancelledUrl))
            {
                return Redirect(cancelledUrl);
            }

            return Ok(new
            {
                message = "پرداخت توسط کاربر لغو شد",
                paymentId = payment.Id,
                status = payment.Status
            });
        }

        if (payment.Status == "Paid")
        {
            var successUrl =
                BuildRedirect(
                    "success",
                    payment.GatewayRefId);

            if (!string.IsNullOrWhiteSpace(successUrl))
            {
                return Redirect(successUrl);
            }

            return Ok(new
            {
                message = "این پرداخت قبلاً تأیید شده است",
                paymentId = payment.Id,
                refId = payment.GatewayRefId,
                status = payment.Status
            });
        }

        if (payment.Status != "Pending")
        {
            return BadRequest(new
            {
                message = "وضعیت فعلی پرداخت قابل تأیید نیست",
                paymentId = payment.Id,
                status = payment.Status
            });
        }

        var verifyResult =
            await _zarinPalService.VerifyPaymentAsync(
                payment.Amount,
                Authority);

        if (!verifyResult.Success)
        {
            var failedUrl =
                BuildRedirect(
                    "failed",
                    code: verifyResult.Code);

            if (!string.IsNullOrWhiteSpace(failedUrl))
            {
                return Redirect(failedUrl);
            }

            return BadRequest(new
            {
                message =
                    verifyResult.Error ??
                    "تأیید پرداخت در زرین‌پال ناموفق بود",
                code = verifyResult.Code
            });
        }

        payment.Status = "Paid";
        payment.GatewayRefId = verifyResult.RefId;
        payment.PaymentDate = DateTime.Now;

        await _context.SaveChangesAsync();

        var finalUrl =
            BuildRedirect(
                "success",
                verifyResult.RefId,
                verifyResult.Code);

        if (!string.IsNullOrWhiteSpace(finalUrl))
        {
            return Redirect(finalUrl);
        }

        return Ok(new
        {
            message = "پرداخت با موفقیت تأیید شد",
            paymentId = payment.Id,
            refId = verifyResult.RefId,
            status = payment.Status,
            code = verifyResult.Code
        });
    }

    // Student - ثبت درخواست پرداخت
[Authorize(Roles = "Student")]
[HttpPost("my/request")]
public async Task<ActionResult<PaymentDto>> CreateStudentPaymentRequest(
    StudentCreatePaymentDto dto)
{
    if (!await _packageAccess.HasPackageAsync(3))
    {
        return StatusCode(StatusCodes.Status403Forbidden, new
        {
            message = "پرداخت آنلاین فقط در پکیج پرداخت آنلاین فعال است."
        });
    }

// ----------------------------------------
    // 1. دریافت شناسه کاربر
    // ----------------------------------------

    var userIdClaim =
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (!int.TryParse(userIdClaim, out var userId))
    {
        return Unauthorized(new
        {
            message = "شناسه کاربر معتبر نیست"
        });
    }


    // ----------------------------------------
    // 2. پیدا کردن پروفایل دانشجو
    // ----------------------------------------

    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.UserId == userId);

    if (student == null)
    {
        return NotFound(new
        {
            message = "پروفایل دانشجویی برای این کاربر پیدا نشد"
        });
    }


    // ----------------------------------------
    // 3. دریافت ثبت نام فقط متعلق به همین دانشجو
    // ----------------------------------------

    var enrollment = await _context.Enrollments
        .Include(e => e.Course)
        .Include(e => e.CoursePartnerOrganization)
        .FirstOrDefaultAsync(e =>
            e.Id == dto.EnrollmentId &&
            e.StudentId == student.Id);

    if (enrollment == null)
    {
        return NotFound(new
        {
            message = "ثبت نام مورد نظر پیدا نشد"
        });
    }


    // ----------------------------------------
    // 4. بررسی وضعیت ثبت نام
    // ----------------------------------------

    if (enrollment.Status == "Cancelled" ||
        enrollment.Status == "Suspended")
    {
        return BadRequest(new
        {
            message = "برای این ثبت نام امکان ثبت درخواست پرداخت وجود ندارد",
            enrollmentStatus = enrollment.Status
        });
    }


    // ----------------------------------------
    // 5. بررسی وجود دوره
    // ----------------------------------------

    if (enrollment.Course == null)
    {
        return BadRequest(new
        {
            message = "دوره مربوط به این ثبت نام پیدا نشد"
        });
    }


    // ----------------------------------------
    // 6. بررسی نوع پرداخت
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
    // 7. محاسبه مبلغ دوره
    // ----------------------------------------

    var coursePrice =
        enrollment.CoursePartnerOrganization?.AgreedPrice
        ?? enrollment.Course.Price;


    // ----------------------------------------
    // 8. محاسبه مجموع پرداخت‌های موفق
    // ----------------------------------------

    var totalPaid = await _context.Payments
        .Where(p =>
            p.EnrollmentId == enrollment.Id &&
            p.Status == "Paid")
        .SumAsync(p => (decimal?)p.Amount) ?? 0;


    var remainingAmount = Math.Max(
        coursePrice - totalPaid,
        0);


    // ----------------------------------------
    // 9. بررسی باقی مانده
    // ----------------------------------------

    if (remainingAmount <= 0)
    {
        return BadRequest(new
        {
            message = "این ثبت نام به طور کامل تسویه شده است",
            coursePrice,
            totalPaid,
            remainingAmount
        });
    }


    // ----------------------------------------
    // 10. بررسی مبلغ درخواست
    // ----------------------------------------

    if (dto.Amount > remainingAmount)
    {
        return BadRequest(new
        {
            message = "مبلغ درخواست نمی‌تواند بیشتر از مبلغ باقی‌مانده باشد",
            coursePrice,
            totalPaid,
            remainingAmount,
            requestedAmount = dto.Amount
        });
    }


    // ----------------------------------------
    // 11. FullPayment باید کل مبلغ باقی مانده باشد
    // ----------------------------------------

    if (dto.PaymentType == "FullPayment" &&
        dto.Amount != remainingAmount)
    {
        return BadRequest(new
        {
            message = "مبلغ پرداخت کامل باید دقیقاً برابر مبلغ باقی‌مانده باشد",
            coursePrice,
            totalPaid,
            remainingAmount,
            requestedAmount = dto.Amount
        });
    }


    // ----------------------------------------
    // 12. جلوگیری از درخواست تکراری قسط
    // ----------------------------------------

    var sameTypeRequestExists = await _context.Payments
        .AnyAsync(p =>
            p.EnrollmentId == enrollment.Id &&
            p.PaymentType == dto.PaymentType &&
            (p.Status == "Pending" || p.Status == "Paid"));

    if (sameTypeRequestExists)
    {
        return BadRequest(new
        {
            message = "برای این نوع پرداخت قبلاً درخواست یا پرداخت ثبت شده است",
            paymentType = dto.PaymentType
        });
    }


    // ----------------------------------------
    // 13. کنترل ترتیب اقساط
    // ----------------------------------------

    if (dto.PaymentType == "SecondInstallment")
    {
        var firstInstallmentExists = await _context.Payments
            .AnyAsync(p =>
                p.EnrollmentId == enrollment.Id &&
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
                p.EnrollmentId == enrollment.Id &&
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


    // ----------------------------------------
    // 14. ثبت درخواست با وضعیت Pending
    // ----------------------------------------

    var payment = new Payment
    {
        EnrollmentId = enrollment.Id,
        Amount = dto.Amount,
        PaymentDate = DateTime.Now,
        PaymentType = dto.PaymentType,
        Description = dto.Description,
        Status = "Pending"
    };

    _context.Payments.Add(payment);

    await _context.SaveChangesAsync();


    // ----------------------------------------
    // 15. نتیجه
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

    return Ok(result);
}

    // ثبت پرداخت جدید توسط کارکنان
[Authorize(Roles = "Admin,EducationStaff,Support")]
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

              if (!int.TryParse(userIdClaim, out var userId))
              {
                  return Unauthorized(new
                  {
                      message = "شناسه کاربر معتبر نیست"
                  });
              }

              if (enrollment.Student == null ||
                  enrollment.Student.SupportUserId != userId)
              {
                  return NotFound(new
                  {
                      message = "ثبت نام مورد نظر پیدا نشد"
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
[Authorize(Roles = "Admin,EducationStaff,Support")]
  [HttpPut("{id}/pay")]
public async Task<IActionResult> Pay(int id)
{
    var payment = await _context.Payments
        .Include(p => p.Enrollment)
        .ThenInclude(e => e!.Course)
        .Include(p => p.Enrollment)
        .ThenInclude(e => e!.CoursePartnerOrganization)
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

          if (!int.TryParse(userIdClaim, out var userId))
          {
              return Unauthorized(new
              {
                  message = "شناسه کاربر معتبر نیست"
              });
          }

          if (payment.Enrollment == null ||
              payment.Enrollment.Student == null ||
              payment.Enrollment.Student.SupportUserId != userId)
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

          if (!int.TryParse(userIdClaim, out var userId))
          {
              return Unauthorized(new
              {
                  message = "شناسه کاربر معتبر نیست"
              });
          }

          var supportStudent = await _context.Students
              .FirstOrDefaultAsync(s =>
                  s.Id == payment.Enrollment.StudentId);

          if (supportStudent == null ||
              supportStudent.SupportUserId != userId)
          {
              return NotFound(new
              {
                  message = "پرداخت مورد نظر پیدا نشد"
              });
          }
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
[Authorize(Roles = "Admin,EducationStaff,Support")]
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
