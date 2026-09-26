using Microsoft.AspNetCore.Authorization;
using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PackageAccessService _packageAccess;
public EnrollmentsController(
        ApplicationDbContext context,
        PackageAccessService packageAccess)
    {
        _context = context;
        _packageAccess = packageAccess;
    }


    // ==========================================
    // قیمت نهایی ثبت نام دانشجو
    // ==========================================

    [Authorize(Roles = "Student")]
    [HttpGet("my/course/{courseId}/price")]
    public async Task<ActionResult<EnrollmentPriceDto>> GetMyCoursePrice(int courseId)
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
            return BadRequest(new
            {
                message = "برای این کاربر پروفایل دانشجویی وجود ندارد"
            });
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive);

        if (course == null)
        {
            return NotFound(new
            {
                message = "دوره پیدا نشد"
            });
        }

        CoursePartnerOrganization? contract = null;

        if (await _packageAccess.HasPackageAsync(4) &&
            student.PartnerOrganizationId.HasValue)
        {
            var today = DateTime.Today;

            contract = await _context.CoursePartnerOrganizations
                .FirstOrDefaultAsync(x =>
                    x.CourseId == courseId &&
                    x.PartnerOrganizationId == student.PartnerOrganizationId.Value &&
                    x.IsActive &&
                    (!x.StartDate.HasValue || x.StartDate.Value.Date <= today) &&
                    (!x.EndDate.HasValue || x.EndDate.Value.Date >= today));
        }

        var finalPrice = contract?.AgreedPrice ?? course.Price;

        var result = new EnrollmentPriceDto
        {
            CourseId = course.Id,
            CoursePrice = course.Price,
            AgreedPrice = contract?.AgreedPrice,
            FinalPrice = finalPrice,
            IsFree = finalPrice <= 0,
            HasOrganizationContract = contract != null,
            PartnerOrganizationId = student.PartnerOrganizationId,
            CoursePartnerOrganizationId = contract?.Id
        };

        return Ok(result);
    }

    // دریافت ثبت نام‌های دانشجوی وارد شده
    [Authorize(Roles = "Student")]
    [HttpGet("my")]
    public async Task<ActionResult<List<EnrollmentDetailDto>>> GetMyEnrollments()
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
            return BadRequest(new
            {
                message = "برای این کاربر پروفایل دانشجویی وجود ندارد"
            });
        }

        var enrollments = await _context.Enrollments
            .Where(e => e.StudentId == student.Id)
            .Select(e => new EnrollmentDetailDto
            {
                Id = e.Id,

                StudentId = e.StudentId,
                StudentName = e.Student != null
                    ? e.Student.FirstName + " " + e.Student.LastName
                    : string.Empty,

                CourseId = e.CourseId,
                CourseTitle = e.Course != null
                    ? e.Course.Title
                    : string.Empty,

                SupportUserId = e.SupportUserId,
                SupportUserName = e.SupportUser != null
                    ? e.SupportUser.FullName
                    : null,

                InstructorId = e.InstructorId,
                InstructorName = e.Instructor != null
                    ? e.Instructor.FullName
                    : null,

                StartDate = e.StartDate,
                Status = e.Status,

                Description = e.Description
            })
            .ToListAsync();

        return Ok(enrollments);
    }

    // Student - گزارش مالی ثبت نام خودش
[Authorize(Roles = "Student")]
[HttpGet("my/{id}/financial")]
public async Task<ActionResult<EnrollmentFinancialDto>> GetMyFinancial(int id)
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

    var enrollment = await _context.Enrollments
        .Include(e => e.Student)
        .Include(e => e.Course)
        .Include(e => e.CoursePartnerOrganization)
        .FirstOrDefaultAsync(e =>
            e.Id == id &&
            e.StudentId == student.Id);

    if (enrollment == null)
    {
        return NotFound(new
        {
            message = "ثبت نام مورد نظر پیدا نشد"
        });
    }

    if (enrollment.Course == null)
    {
        return BadRequest(new
        {
            message = "دوره مربوط به این ثبت نام وجود ندارد"
        });
    }

    var payments = await _context.Payments
        .Where(p => p.EnrollmentId == id)
        .Select(p => new
        {
            p.Amount,
            p.Status,
            p.DueDate
        })
        .ToListAsync();

    var totalPaid = payments
        .Where(p => string.Equals(p.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        .Sum(p => p.Amount);

    var coursePrice = enrollment.CoursePartnerOrganization?.AgreedPrice ?? enrollment.Course.Price;
    var remainingAmount = Math.Max(coursePrice - totalPaid, 0);

    string paymentStatus;

    if (totalPaid <= 0)
    {
        paymentStatus = "Unpaid";
    }
    else if (totalPaid < coursePrice)
    {
        paymentStatus = "PartiallyPaid";
    }
    else if (totalPaid == coursePrice)
    {
        paymentStatus = "Paid";
    }
    else
    {
        paymentStatus = "Overpaid";
    }

    var today = DateTime.Today;

    var unpaidPayments = payments
        .Where(p => !string.Equals(p.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        .Where(p => p.DueDate.HasValue)
        .OrderBy(p => p.DueDate)
        .ToList();

    var nextPayment = unpaidPayments.FirstOrDefault();

    var overdueInstallmentCount = payments
        .Count(p =>
            !string.Equals(p.Status, "Paid", StringComparison.OrdinalIgnoreCase) &&
            p.DueDate.HasValue &&
            p.DueDate.Value.Date < today);

    var result = new EnrollmentFinancialDto
    {
        EnrollmentId = enrollment.Id,
        StudentName = enrollment.Student != null
            ? enrollment.Student.FirstName + " " + enrollment.Student.LastName
            : string.Empty,
        CourseTitle = enrollment.Course.Title,
        CoursePrice = coursePrice,
        TotalPaid = totalPaid,
        RemainingAmount = remainingAmount,
        PaymentStatus = paymentStatus,
        NextDueDate = nextPayment?.DueDate,
        NextDueStatus = nextPayment == null
            ? string.Empty
            : nextPayment.DueDate!.Value.Date < today
                ? "Overdue"
                : nextPayment.DueDate.Value.Date == today
                    ? "Due"
                    : "Future",
        OverdueInstallmentCount = overdueInstallmentCount
    };

    return Ok(result);
}

  // Student - جزئیات مالی ثبت نام خودش
[Authorize(Roles = "Student")]
[HttpGet("my/{id}/financial-details")]
public async Task<ActionResult<EnrollmentFinancialDetailDto>> GetMyFinancialDetails(int id)
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

    var enrollment = await _context.Enrollments
        .Include(e => e.Student)
        .Include(e => e.CoursePartnerOrganization)
        .Include(e => e.Course)
        .FirstOrDefaultAsync(e =>
            e.Id == id &&
            e.StudentId == student.Id);

    if (enrollment == null)
    {
        return NotFound(new
        {
            message = "ثبت نام مورد نظر پیدا نشد"
        });
    }

    if (enrollment.Course == null)
    {
        return BadRequest(new
        {
            message = "دوره مربوط به این ثبت نام وجود ندارد"
        });
    }

    var payments = await _context.Payments
        .Where(p => p.EnrollmentId == id)
        .OrderBy(p => p.DueDate ?? DateTime.MaxValue)
        .ThenBy(p => p.PaymentDate)
        .Select(p => new PaymentItemDto
        {
            Id = p.Id,
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            DueDate = p.DueDate,
            PaymentType = p.PaymentType,
            Description = p.Description,
            Status = p.Status
        })
        .ToListAsync();

    var today = DateTime.Today;

    foreach (var payment in payments)
    {
        if (string.Equals(payment.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            payment.DueStatus = "Paid";
        }
        else if (!payment.DueDate.HasValue)
        {
            payment.DueStatus = "NoDueDate";
        }
        else if (payment.DueDate.Value.Date < today)
        {
            payment.DueStatus = "Overdue";
        }
        else if (payment.DueDate.Value.Date == today)
        {
            payment.DueStatus = "Due";
        }
        else
        {
            payment.DueStatus = "Future";
        }
    }

    var totalPaid = payments
        .Where(p => string.Equals(p.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        .Sum(p => p.Amount);

    var coursePrice = enrollment.CoursePartnerOrganization?.AgreedPrice ?? enrollment.Course.Price;

    var remainingAmount = Math.Max(coursePrice - totalPaid, 0);

    string paymentStatus;

    if (totalPaid <= 0)
    {
        paymentStatus = "Unpaid";
    }
    else if (totalPaid < coursePrice)
    {
        paymentStatus = "PartiallyPaid";
    }
    else if (totalPaid == coursePrice)
    {
        paymentStatus = "Paid";
    }
    else
    {
        paymentStatus = "Overpaid";
    }

    var unpaidPayments = payments
        .Where(p => !string.Equals(p.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        .Where(p => p.DueDate.HasValue)
        .OrderBy(p => p.DueDate)
        .ToList();

    var nextPayment = unpaidPayments.FirstOrDefault();

    var overdueInstallmentCount = payments
        .Count(p => p.DueStatus == "Overdue");

    var result = new EnrollmentFinancialDetailDto
    {
        EnrollmentId = enrollment.Id,

        StudentName = enrollment.Student != null
            ? enrollment.Student.FirstName + " " + enrollment.Student.LastName
            : string.Empty,

        CourseTitle = enrollment.Course.Title,

        CoursePrice = coursePrice,

        TotalPaid = totalPaid,

        RemainingAmount = remainingAmount,

        PaymentStatus = paymentStatus,

        NextDueDate = nextPayment?.DueDate,

        NextDueStatus = nextPayment?.DueStatus ?? string.Empty,

        OverdueInstallmentCount = overdueInstallmentCount,

        Payments = payments
    };

    return Ok(result);
}

    // دریافت ثبت نام‌های دانشجویان اختصاص یافته به Support
[Authorize(Roles = "Support")]
[HttpGet("support/my")]
public async Task<ActionResult<List<EnrollmentDetailDto>>> GetMySupportEnrollments()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (!int.TryParse(userIdClaim, out var supportUserId))
    {
        return Unauthorized(new
        {
            message = "شناسه کاربر معتبر نیست"
        });
    }

    var enrollments = await _context.Enrollments
        .Where(e =>
            e.Student != null &&
            e.Student.SupportUserId == supportUserId)
        .Select(e => new EnrollmentDetailDto
        {
            Id = e.Id,

            StudentId = e.StudentId,
            StudentName = e.Student != null
                ? e.Student.FirstName + " " + e.Student.LastName
                : string.Empty,

            CourseId = e.CourseId,
            CourseTitle = e.Course != null
                ? e.Course.Title
                : string.Empty,

            SupportUserId = e.SupportUserId,
            SupportUserName = e.SupportUser != null
                ? e.SupportUser.FullName
                : null,

            InstructorId = e.InstructorId,
            InstructorName = e.Instructor != null
                ? e.Instructor.FullName
                : null,

            StartDate = e.StartDate,
            Status = e.Status,

            Description = e.Description
        })
        .ToListAsync();

    return Ok(enrollments);
}

    // دریافت لیست ثبت نام ها با اطلاعات دانشجو، دوره، پشتیبان و استاد
    
    [Authorize(Roles = "Admin,EducationStaff,Marketer,Support,Instructor,Student")]
  [HttpGet]
  // دریافت لیست ثبت نام ها با اطلاعات دانشجو، دوره، پشتیبان، استاد و وضعیت مالی
public async Task<ActionResult<List<EnrollmentDto>>> GetEnrollments()
{
    var query = _context.Enrollments
        .Include(e => e.Student)
        .Include(e => e.Course)
        .Include(e => e.SupportUser)
        .Include(e => e.Instructor)
        .Include(e => e.CoursePartnerOrganization)
            .ThenInclude(cpo => cpo!.PartnerOrganization)
        .AsQueryable();

    // Student → فقط ثبت‌نام‌های خودش
    if (User.IsInRole("Student"))
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

        query = query.Where(e => e.StudentId == student.Id);
    }

    // Support → فقط ثبت‌نام دانشجویان خودش
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

        query = query.Where(e =>
            e.Student != null &&
            e.Student.SupportUserId == userId);
    }

      // Marketer → فقط ثبت‌نام دانشجویان ارجاع‌شده توسط خودش
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

          query = query.Where(e =>
              e.Student != null &&
              e.Student.MarketingUserId == userId);
      }

      // Instructor → فقط ثبت‌نام‌های دوره‌های خودش
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

          query = query.Where(e =>
              e.Course != null &&
              e.Course.InstructorId == userId);
      }

      // دریافت ثبت‌نام‌ها
    var enrollmentEntities = await query.ToListAsync();

    // دریافت مجموع پرداخت‌های تاییدشده برای ثبت‌نام‌های موجود
    var enrollmentIds = enrollmentEntities
        .Select(e => e.Id)
        .ToList();

    var paidAmounts = await _context.Payments
        .Where(p =>
            enrollmentIds.Contains(p.EnrollmentId) &&
            p.Status == "Paid")
        .GroupBy(p => p.EnrollmentId)
        .Select(g => new
        {
            EnrollmentId = g.Key,
            TotalPaid = g.Sum(p => p.Amount)
        })
        .ToDictionaryAsync(
            x => x.EnrollmentId,
            x => x.TotalPaid);

    var enrollments = enrollmentEntities
        .Select(e =>
        {
            var coursePrice =
                e.CoursePartnerOrganization?.AgreedPrice
                ?? e.Course?.Price
                ?? 0;

            var totalPaid =
                paidAmounts.TryGetValue(e.Id, out var paid)
                    ? paid
                    : 0;

            var remainingAmount =
                Math.Max(coursePrice - totalPaid, 0);

            string paymentStatus;

            if (totalPaid <= 0)
            {
                paymentStatus = "Unpaid";
            }
            else if (totalPaid < coursePrice)
            {
                paymentStatus = "PartiallyPaid";
            }
            else if (totalPaid == coursePrice)
            {
                paymentStatus = "Paid";
            }
            else
            {
                paymentStatus = "Overpaid";
            }

            return new EnrollmentDto
            {
                Id = e.Id,

                StudentId = e.StudentId,

                StudentName = e.Student != null
                    ? e.Student.FirstName + " " + e.Student.LastName
                    : string.Empty,

                CourseId = e.CourseId,

                CourseTitle = e.Course != null
                    ? e.Course.Title
                    : string.Empty,

                CoursePrice = coursePrice,

                TotalPaid = totalPaid,

                RemainingAmount = remainingAmount,

                PaymentStatus = paymentStatus,

                DeliveryType = e.Course?.DeliveryType ?? string.Empty,

                HasPaymentWarning =
                    string.Equals(e.Course?.DeliveryType, "InPerson", StringComparison.OrdinalIgnoreCase)
                    && remainingAmount > 0,

                SupportUserId = e.SupportUserId,

                SupportUserName = e.SupportUser != null
                    ? e.SupportUser.FullName
                    : null,

                InstructorId = e.InstructorId,

                InstructorName = e.Instructor != null
                    ? e.Instructor.FullName
                    : null,

                StartDate = e.StartDate,

                Status = e.Status,

                Description = e.Description,

                CoursePartnerOrganization =
                    e.CoursePartnerOrganization == null
                        ? null
                        : new CoursePartnerOrganizationDto
                        {
                            Id = e.CoursePartnerOrganization.Id,

                            CourseId =
                                e.CoursePartnerOrganization.CourseId,

                            PartnerOrganizationId =
                                e.CoursePartnerOrganization.PartnerOrganizationId,

                            ContractNumber =
                                e.CoursePartnerOrganization.ContractNumber,

                            AgreedPrice =
                                e.CoursePartnerOrganization.AgreedPrice,

                            StartDate =
                                e.CoursePartnerOrganization.StartDate,

                            EndDate =
                                e.CoursePartnerOrganization.EndDate,

                            IsActive =
                                e.CoursePartnerOrganization.IsActive,

                            Description =
                                e.CoursePartnerOrganization.Description,

                            PartnerOrganization =
                                e.CoursePartnerOrganization.PartnerOrganization == null
                                    ? null
                                    : new PartnerOrganizationDto
                                    {
                                        Id =
                                            e.CoursePartnerOrganization
                                                .PartnerOrganization.Id,

                                        Name =
                                            e.CoursePartnerOrganization
                                                .PartnerOrganization.Name
                                    }
                        }
            };
        })
        .ToList();

    return Ok(enrollments);
}

    // گزارش مالی یک ثبت نام
    [Authorize(Roles = "Admin,EducationStaff,Marketer,Support,Instructor,Student")]
[HttpGet("{id}/financial")]
public async Task<ActionResult<EnrollmentFinancialDto>> GetFinancial(int id)
{
    var enrollment = await _context.Enrollments
        .Include(e => e.CoursePartnerOrganization)
        .Include(e => e.Student)
        .Include(e => e.Course)
        .FirstOrDefaultAsync(e => e.Id == id);

    if (enrollment == null)
    {
        return NotFound(new
        {
            message = "ثبت نام مورد نظر پیدا نشد"
        });
    }

    // Student → فقط اطلاعات مالی ثبت نام خودش
    if (User.IsInRole("Student"))
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

        if (enrollment.StudentId != student.Id)
        {
            return NotFound(new
            {
                message = "ثبت نام مورد نظر پیدا نشد"
            });
        }
    }

    // Support → فقط اطلاعات مالی دانشجویان خودش
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

    if (enrollment.Course == null)
    {
        return BadRequest(new
        {
            message = "دوره مربوط به این ثبت نام وجود ندارد"
        });
    }

    var totalPaid = await _context.Payments
        .Where(p =>
            p.EnrollmentId == id &&
            p.Status == "Paid")
        .SumAsync(p => (decimal?)p.Amount) ?? 0;

    var coursePrice = enrollment.CoursePartnerOrganization?.AgreedPrice ?? enrollment.Course.Price;
    var remainingAmount = Math.Max(coursePrice - totalPaid, 0);

    string paymentStatus;

    if (totalPaid <= 0)
    {
        paymentStatus = "Unpaid";
    }
    else if (totalPaid < coursePrice)
    {
        paymentStatus = "PartiallyPaid";
    }
    else if (totalPaid == coursePrice)
    {
        paymentStatus = "Paid";
    }
    else
    {
        paymentStatus = "Overpaid";
    }

    var result = new EnrollmentFinancialDto
    {
        EnrollmentId = enrollment.Id,
        StudentName = enrollment.Student != null
            ? enrollment.Student.FirstName + " " + enrollment.Student.LastName
            : string.Empty,
        CourseTitle = enrollment.Course.Title,
        CoursePrice = coursePrice,
        TotalPaid = totalPaid,
        RemainingAmount = remainingAmount,
        PaymentStatus = paymentStatus
    };

    return Ok(result);
}

    // جزئیات مالی ثبت نام به همراه لیست پرداخت‌ها
    [Authorize(Roles = "Admin,EducationStaff,Marketer,Support,Instructor,Student")]
[HttpGet("{id}/financial-details")]
public async Task<ActionResult<EnrollmentFinancialDetailDto>> GetFinancialDetails(int id)
{
    var enrollment = await _context.Enrollments
        .Include(e => e.Student)
        .Include(e => e.Course)
        .Include(e => e.CoursePartnerOrganization)
        .FirstOrDefaultAsync(e => e.Id == id);

    if (enrollment == null)
    {
        return NotFound(new
        {
            message = "ثبت نام مورد نظر پیدا نشد"
        });
    }

    // Student → فقط اطلاعات مالی ثبت نام خودش
    if (User.IsInRole("Student"))
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

        if (enrollment.StudentId != student.Id)
        {
            return NotFound(new
            {
                message = "ثبت نام مورد نظر پیدا نشد"
            });
        }
    }

    // Support → فقط اطلاعات مالی دانشجویان خودش
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

    if (enrollment.Course == null)
    {
        return BadRequest(new
        {
            message = "دوره مربوط به این ثبت نام وجود ندارد"
        });
    }

    var payments = await _context.Payments
        .Where(p => p.EnrollmentId == id)
        .OrderBy(p => p.PaymentDate)
        .Select(p => new PaymentItemDto
        {
            Id = p.Id,
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            PaymentType = p.PaymentType,
            Description = p.Description,
            Status = p.Status
        })
        .ToListAsync();

    var totalPaid = payments
        .Where(p => p.Status == "Paid")
        .Sum(p => p.Amount);

    var coursePrice = enrollment.CoursePartnerOrganization?.AgreedPrice ?? enrollment.Course.Price;
    var remainingAmount = Math.Max(coursePrice - totalPaid, 0);

    string paymentStatus;

    if (totalPaid <= 0)
    {
        paymentStatus = "Unpaid";
    }
    else if (totalPaid < coursePrice)
    {
        paymentStatus = "PartiallyPaid";
    }
    else if (totalPaid == coursePrice)
    {
        paymentStatus = "Paid";
    }
    else
    {
        paymentStatus = "Overpaid";
    }

    var result = new EnrollmentFinancialDetailDto
    {
        EnrollmentId = enrollment.Id,
        StudentName = enrollment.Student != null
            ? enrollment.Student.FirstName + " " + enrollment.Student.LastName
            : string.Empty,
        CourseTitle = enrollment.Course.Title,
        CoursePrice = coursePrice,
        TotalPaid = totalPaid,
        RemainingAmount = remainingAmount,
        PaymentStatus = paymentStatus,
        Payments = payments
    };

    return Ok(result);
}

// ثبت نام دانشجو در دوره
[Authorize(Roles = "Admin,EducationStaff,Marketer,Support,Student")]
[HttpPost]
public async Task<ActionResult<EnrollmentDto>> Create(CreateEnrollmentDto dto)
{
    // بررسی شناسه کاربر از JWT
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (!int.TryParse(userIdClaim, out var userId))
    {
        return Unauthorized(new
        {
            message = "شناسه کاربر معتبر نیست"
        });
    }

    Student? currentStudent = null;
    var isStudent = User.IsInRole("Student");

    // اگر کاربر Student باشد،
    // StudentId از روی UserId تعیین می‌شود
    if (isStudent)
    {
        currentStudent = await _context.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (currentStudent == null)
        {
            return BadRequest(new
            {
                message = "برای این کاربر پروفایل دانشجویی وجود ندارد"
            });
        }

        dto.StudentId = currentStudent.Id;
    }

    // Student نباید پشتیبان یا مدرس را خودش تعیین کند
    if (isStudent &&
        (dto.SupportUserId.HasValue || dto.InstructorId.HasValue))
    {
        return BadRequest(new
        {
            message = "دانشجو مجاز به تعیین پشتیبان یا مدرس ثبت نام نیست."
        });
    }

    // Marketer → فقط برای دانشجویان ارجاع‌شده توسط خودش
    if (User.IsInRole("Marketer"))
    {
        var relatedStudent = await _context.Students
            .FirstOrDefaultAsync(s =>
                s.Id == dto.StudentId &&
                s.MarketingUserId == userId);

        if (relatedStudent == null)
        {
            return NotFound(new
            {
                message = "این دانشجو در فهرست ارجاع‌های شما قرار ندارد."
            });
        }

        if (dto.SupportUserId.HasValue ||
            dto.InstructorId.HasValue)
        {
            return BadRequest(new
            {
                message = "بازاریاب مجاز به تعیین پشتیبان یا مدرس ثبت نام نیست."
            });
        }
    }

    // Support → فقط برای دانشجویان اختصاص‌یافته به خودش
    if (User.IsInRole("Support"))
    {
        var relatedStudent = await _context.Students
            .FirstOrDefaultAsync(s =>
                s.Id == dto.StudentId &&
                s.SupportUserId == userId);

        if (relatedStudent == null)
        {
            return NotFound(new
            {
                message = "این دانشجو به شما اختصاص داده نشده است."
            });
        }

        if (dto.SupportUserId.HasValue &&
            dto.SupportUserId.Value != userId)
        {
            return BadRequest(new
            {
                message = "پشتیبان نمی‌تواند ثبت نام را به پشتیبان دیگری اختصاص دهد."
            });
        }

        dto.SupportUserId = userId;
    }

    // بررسی وجود دانشجو
    var student = currentStudent;

    if (student == null)
    {
        student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == dto.StudentId);
    }

    if (student == null)
    {
        return BadRequest(new
        {
            message = "دانشجوی مورد نظر وجود ندارد"
        });
    }

    // بررسی وجود دوره
    var course = await _context.Courses
        .FirstOrDefaultAsync(c => c.Id == dto.CourseId);

    if (course == null)
    {
        return BadRequest(new
        {
            message = "Course not found."
        });
    }

    // Package 1 → فقط دوره حضوری
    if (!await _packageAccess.HasPackageAsync(2) &&
        !string.Equals(
            course.DeliveryType,
            "InPerson",
            StringComparison.OrdinalIgnoreCase))
    {
        return StatusCode(
            StatusCodes.Status403Forbidden,
            new
            {
                message = "Package 1 supports in-person enrollment only."
            });
    }

    // بررسی ثبت نام تکراری دانشجو در دوره
    var duplicateEnrollment = await _context.Enrollments
        .AnyAsync(e =>
            e.StudentId == dto.StudentId &&
            e.CourseId == dto.CourseId &&
            e.Status == "Active");

    if (duplicateEnrollment)
    {
        return BadRequest(new
        {
            message = "این دانشجو قبلاً در این دوره ثبت نام کرده است"
        });
    }

    // =========================================================
    // منطق قرارداد سازمانی
    // =========================================================

    // اگر Student باشد:
    // قرارداد نباید از سمت کاربر انتخاب شود؛
    // قرارداد باید بر اساس سازمان خود دانشجو تعیین شود.
    if (isStudent)
    {
        // قرارداد سازمانی فقط در Package 4 قابل استفاده است
        if (!await _packageAccess.HasPackageAsync(4))
        {
            if (dto.CoursePartnerOrganizationId.HasValue)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "Organization enrollment is available only in Package 4."
                    });
            }

            dto.CoursePartnerOrganizationId = null;
        }
        else
        {
            // دانشجو سازمان طرف قرارداد دارد
            if (student.PartnerOrganizationId.HasValue)
            {
                var today = DateTime.Today;

                var ownOrganizationContract =
                    await _context.CoursePartnerOrganizations
                        .Where(x =>
                            x.CourseId == dto.CourseId &&
                            x.PartnerOrganizationId ==
                                student.PartnerOrganizationId.Value &&
                            x.IsActive &&
                            (!x.StartDate.HasValue ||
                             x.StartDate.Value.Date <= today) &&
                            (!x.EndDate.HasValue ||
                             x.EndDate.Value.Date >= today))
                        .OrderBy(x => x.Id)
                        .FirstOrDefaultAsync();

                // اگر دانشجو شناسه یک قرارداد را دستی ارسال کرده،
                // باید دقیقاً همان قرارداد سازمان خودش باشد.
                if (dto.CoursePartnerOrganizationId.HasValue)
                {
                    if (ownOrganizationContract == null)
                    {
                        return BadRequest(new
                        {
                            message =
                                "برای سازمان شما قرارداد فعالی برای این دوره وجود ندارد."
                        });
                    }

                    if (dto.CoursePartnerOrganizationId.Value !=
                        ownOrganizationContract.Id)
                    {
                        return BadRequest(new
                        {
                            message =
                                "دانشجو فقط مجاز به استفاده از قرارداد سازمان خودش است."
                        });
                    }
                }

                // تعیین قرارداد توسط سرور
                dto.CoursePartnerOrganizationId =
                    ownOrganizationContract?.Id;
            }
            else
            {
                // دانشجو به هیچ سازمانی وابسته نیست،
                // بنابراین نمی‌تواند قرارداد سازمانی ارسال کند.
                if (dto.CoursePartnerOrganizationId.HasValue)
                {
                    return BadRequest(new
                    {
                        message =
                            "این دانشجو به سازمان طرف قرارداد اختصاص داده نشده است."
                    });
                }

                dto.CoursePartnerOrganizationId = null;
            }
        }
    }
    else
    {
        // =====================================================
        // منطق قبلی برای کاربران مدیریتی
        // =====================================================

        if (dto.CoursePartnerOrganizationId.HasValue &&
            !await _packageAccess.HasPackageAsync(4))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message = "Organization enrollment is available only in Package 4."
                });
        }

        if (dto.CoursePartnerOrganizationId.HasValue)
        {
            var coursePartnerOrganization =
                await _context.CoursePartnerOrganizations
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.CoursePartnerOrganizationId.Value);

            if (coursePartnerOrganization == null)
            {
                return BadRequest(new
                {
                    message = "ارتباط دوره و سازمان طرف قرارداد پیدا نشد."
                });
            }

            if (coursePartnerOrganization.CourseId != dto.CourseId)
            {
                return BadRequest(new
                {
                    message = "سازمان طرف قرارداد مربوط به این دوره نیست."
                });
            }

            if (!coursePartnerOrganization.IsActive)
            {
                return BadRequest(new
                {
                    message = "قرارداد سازمانی این دوره غیرفعال است."
                });
            }
        }
    }

    // بررسی وجود پشتیبان
    if (dto.SupportUserId.HasValue)
    {
        var supportExists = await _context.Users
            .AnyAsync(u =>
                u.Id == dto.SupportUserId.Value &&
                u.IsActive &&
                u.Role == "Support");

        if (!supportExists)
        {
            return BadRequest(new
            {
                message = "پشتیبان آموزشی مورد نظر وجود ندارد"
            });
        }
    }

    // بررسی وجود استاد
    if (dto.InstructorId.HasValue)
    {
        var instructorExists = await _context.Users
            .AnyAsync(u =>
                u.Id == dto.InstructorId.Value &&
                u.IsActive &&
                u.Role == "Instructor");

        if (!instructorExists)
        {
            return BadRequest(new
            {
                message = "استاد مورد نظر وجود ندارد"
            });
        }
    }

    // ایجاد ثبت نام
    var enrollment = new Enrollment
    {
        StudentId = dto.StudentId,
        CoursePartnerOrganizationId = dto.CoursePartnerOrganizationId,
        CourseId = dto.CourseId,
        SupportUserId = isStudent ? null : dto.SupportUserId,
        InstructorId = isStudent ? course.InstructorId : dto.InstructorId,
        StartDate = isStudent ? DateTime.Now : dto.StartDate,
        Status = isStudent ? "Active" : dto.Status,
        Description = dto.Description
    };

    _context.Enrollments.Add(enrollment);

    await _context.SaveChangesAsync();

    // آماده سازی نتیجه
    var result = new EnrollmentDto
    {
        Id = enrollment.Id,
        StudentId = enrollment.StudentId,
        CourseId = enrollment.CourseId,
        SupportUserId = enrollment.SupportUserId,
        InstructorId = enrollment.InstructorId,
        StartDate = enrollment.StartDate,
        Status = enrollment.Status,
        Description = enrollment.Description
    };

    return result;
}
        // ویرایش ثبت نام
    [Authorize(Roles = "Admin,EducationStaff,Support")]
      [HttpPut("{id}")]
    public async Task<ActionResult<EnrollmentDto>> Update(
        int id,
        UpdateEnrollmentDto dto)
    {
        // پیدا کردن ثبت نام
        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.CoursePartnerOrganization)
                .ThenInclude(cpo => cpo!.PartnerOrganization)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
        {
            return NotFound(new
            {
                message = "ثبت نام مورد نظر پیدا نشد"
            });
        }

        

          // Support → فقط ثبت‌نام دانشجویان اختصاص‌یافته به خودش
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

              if (dto.SupportUserId.HasValue &&
                  dto.SupportUserId.Value != userId)
              {
                  return BadRequest(new
                  {
                      message = "پشتیبان نمی‌تواند ثبت نام را به پشتیبان دیگری اختصاص دهد."
                  });
              }

              dto.SupportUserId = userId;
          }
// وضعیت های مجاز ثبت نام
        var allowedStatuses = new[]
        {
            "Active",
            "Completed",
            "Cancelled",
            "Suspended"
        };

        if (!allowedStatuses.Contains(dto.Status))
        {
            return BadRequest(new
            {
                message = "وضعیت ثبت نام نامعتبر است"
            });
        }

        // بررسی وجود پشتیبان
        if (dto.SupportUserId.HasValue)
{
    var supportExists = await _context.Users
        .AnyAsync(u =>
            u.Id == dto.SupportUserId.Value &&
            u.IsActive &&
            u.Role == "Support");

            if (!supportExists)
            {
                return BadRequest(new
                {
                    message = "پشتیبان آموزشی مورد نظر وجود ندارد یا غیرفعال است"
                });
            }
        }

        // بررسی وجود استاد
        if (dto.InstructorId.HasValue)
        {
            var instructorExists = await _context.Users
                .AnyAsync(u =>
                    u.Id == dto.InstructorId.Value &&
                      u.IsActive &&
                      u.Role == "Instructor");

            if (!instructorExists)
            {
                return BadRequest(new
                {
                    message = "استاد مورد نظر وجود ندارد یا غیرفعال است"
                });
            }
        }

        // بررسی تغییر قرارداد سازمانی
        if (dto.CoursePartnerOrganizationId !=
            enrollment.CoursePartnerOrganizationId)
        {
            if (!await _packageAccess.HasPackageAsync(4))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "Organization enrollment changes are available only in Package 4."
                    });
            }

            // آیا برای این ثبت نام پرداختی انجام شده؟
            var hasPaidPayment = await _context.Payments
                .AnyAsync(p =>
                    p.EnrollmentId == enrollment.Id &&
                    p.Status == "Paid");

            if (hasPaidPayment)
            {
                return BadRequest(new
                {
                    message =
                        "پس از ثبت پرداخت، تغییر قرارداد سازمانی ثبت نام مجاز نیست."
                });
            }

            // اگر قرارداد جدید انتخاب شده، اعتبار آن بررسی شود
            if (dto.CoursePartnerOrganizationId.HasValue)
            {
                var coursePartnerOrganization =
                    await _context.CoursePartnerOrganizations
                        .FirstOrDefaultAsync(x =>
                            x.Id == dto.CoursePartnerOrganizationId.Value);

                if (coursePartnerOrganization == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "ارتباط دوره و سازمان طرف قرارداد پیدا نشد."
                    });
                }

                if (coursePartnerOrganization.CourseId !=
                    enrollment.CourseId)
                {
                    return BadRequest(new
                    {
                        message =
                            "سازمان طرف قرارداد مربوط به این دوره نیست."
                    });
                }

                if (!coursePartnerOrganization.IsActive)
                {
                    return BadRequest(new
                    {
                        message =
                            "قرارداد سازمانی این دوره غیرفعال است."
                    });
                }

                if (!coursePartnerOrganization.AgreedPrice.HasValue)
                {
                    return BadRequest(new
                    {
                        message =
                            "برای قرارداد انتخاب‌شده قیمت توافقی تعیین نشده است."
                    });
                }
            }

            enrollment.CoursePartnerOrganizationId =
                dto.CoursePartnerOrganizationId;
        }

        // به روز رسانی اطلاعات ثبت نام
        if (dto.SupportUserId.HasValue)
{
    enrollment.SupportUserId = dto.SupportUserId;
}

if (dto.InstructorId.HasValue)
{
    enrollment.InstructorId = dto.InstructorId;
}
        enrollment.StartDate = dto.StartDate;
        enrollment.Status = dto.Status;
        enrollment.Description =
            string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();

        await _context.SaveChangesAsync();

        // ساخت نتیجه
        var result = new EnrollmentDto
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            StudentName = enrollment.Student != null
                ? enrollment.Student.FirstName + " " +
                  enrollment.Student.LastName
                : string.Empty,
            CourseId = enrollment.CourseId,
            CourseTitle = enrollment.Course?.Title,
            SupportUserId = enrollment.SupportUserId,
            InstructorId = enrollment.InstructorId,
            StartDate = enrollment.StartDate,
            Status = enrollment.Status,
            Description = enrollment.Description,

            CoursePartnerOrganization =
                enrollment.CoursePartnerOrganization == null
                    ? null
                    : new CoursePartnerOrganizationDto
                    {
                        Id = enrollment.CoursePartnerOrganization.Id,
                        CourseId =
                            enrollment.CoursePartnerOrganization.CourseId,
                        PartnerOrganizationId =
                            enrollment.CoursePartnerOrganization
                                .PartnerOrganizationId,
                        ContractNumber =
                            enrollment.CoursePartnerOrganization
                                .ContractNumber,
                        AgreedPrice =
                            enrollment.CoursePartnerOrganization.AgreedPrice,
                        StartDate =
                            enrollment.CoursePartnerOrganization.StartDate,
                        EndDate =
                            enrollment.CoursePartnerOrganization.EndDate,
                        IsActive =
                            enrollment.CoursePartnerOrganization.IsActive,
                        Description =
                            enrollment.CoursePartnerOrganization.Description,

                        PartnerOrganization =
                            enrollment.CoursePartnerOrganization
                                .PartnerOrganization == null
                                ? null
                                : new PartnerOrganizationDto
                                {
                                    Id =
                                        enrollment
                                            .CoursePartnerOrganization
                                            .PartnerOrganization.Id,
                                    Name =
                                        enrollment
                                            .CoursePartnerOrganization
                                            .PartnerOrganization.Name
                                }
                    }
        };

        return Ok(result);
    }
}
