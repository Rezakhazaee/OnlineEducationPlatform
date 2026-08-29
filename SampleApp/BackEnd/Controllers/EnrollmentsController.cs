using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EnrollmentsController(ApplicationDbContext context)
    {
        _context = context;
    }


    // دریافت لیست ثبت نام ها با اطلاعات دانشجو، دوره، پشتیبان و استاد
    [HttpGet]
    public async Task<List<EnrollmentDetailDto>> Get()
    {
        return await _context.Enrollments
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
                Status = e.Status
            })
            .ToListAsync();
    }


    // گزارش مالی یک ثبت نام
    [HttpGet("{id}/financial")]
    public async Task<ActionResult<EnrollmentFinancialDto>> GetFinancial(int id)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
        {
            return NotFound(new
            {
                message = "ثبت نام مورد نظر وجود ندارد"
            });
        }

        if (enrollment.Course == null)
        {
            return BadRequest(new
            {
                message = "دوره مربوط به این ثبت نام وجود ندارد"
            });
        }


        // فقط پرداخت‌های Paid محاسبه می‌شوند
        var totalPaid = await _context.Payments
            .Where(p =>
                p.EnrollmentId == id &&
                p.Status == "Paid")
            .SumAsync(p => (decimal?)p.Amount) ?? 0;


        var coursePrice = enrollment.Course.Price;

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


        return result;
    }


    // ثبت نام دانشجو در دوره
    [HttpPost]
    public async Task<ActionResult<EnrollmentDto>> Create(CreateEnrollmentDto dto)
    {
        // بررسی وجود دانشجو
        var studentExists = await _context.Students
            .AnyAsync(s => s.Id == dto.StudentId);

        if (!studentExists)
        {
            return BadRequest(new
            {
                message = "دانشجوی مورد نظر وجود ندارد"
            });
        }


        // بررسی وجود دوره
        var courseExists = await _context.Courses
            .AnyAsync(c => c.Id == dto.CourseId);

        if (!courseExists)
        {
            return BadRequest(new
            {
                message = "دوره مورد نظر وجود ندارد"
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


        // بررسی وجود پشتیبان آموزشی، در صورت ارسال
        if (dto.SupportUserId.HasValue)
        {
            var supportExists = await _context.Users
                .AnyAsync(u => u.Id == dto.SupportUserId.Value);

            if (!supportExists)
            {
                return BadRequest(new
                {
                    message = "پشتیبان آموزشی مورد نظر وجود ندارد"
                });
            }
        }


        // بررسی وجود استاد، در صورت ارسال
        if (dto.InstructorId.HasValue)
        {
            var instructorExists = await _context.Users
                .AnyAsync(u => u.Id == dto.InstructorId.Value);

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
            CourseId = dto.CourseId,
            SupportUserId = dto.SupportUserId,
            InstructorId = dto.InstructorId,
            StartDate = dto.StartDate,
            Status = dto.Status
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
            Status = enrollment.Status
        };


        return result;
    }
}