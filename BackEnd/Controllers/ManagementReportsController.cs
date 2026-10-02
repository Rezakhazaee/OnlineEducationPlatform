using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ManagementReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ManagementReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? courseId,
        [FromQuery] int? instructorId,
        [FromQuery] string? deliveryType,
        [FromQuery] string? status)
    {
        var query = _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.Instructor)
            .Where(e => e.Course != null)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.StartDate >= from.Value);

        if (to.HasValue)
        {
            var endDate = to.Value.Date.AddDays(1);
            query = query.Where(e => e.StartDate < endDate);
        }

        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);

        if (instructorId.HasValue)
            query = query.Where(e => e.InstructorId == instructorId.Value);

        if (!string.IsNullOrWhiteSpace(deliveryType))
            query = query.Where(e =>
                e.Course!.DeliveryType == deliveryType);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e =>
                e.Status == status);

        var enrollments = await query.ToListAsync();

        var enrollmentIds = enrollments
            .Select(e => e.Id)
            .ToList();

        var paidByEnrollment = await _context.Payments
            .AsNoTracking()
            .Where(p =>
                enrollmentIds.Contains(p.EnrollmentId) &&
                p.Status == "Paid")
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new
            {
                EnrollmentId = g.Key,
                Amount = g.Sum(p => p.Amount)
            })
            .ToDictionaryAsync(
                x => x.EnrollmentId,
                x => x.Amount);

        var expectedAmount = enrollments.Sum(e =>
            e.Course?.Price ?? 0);

        var totalPaid = enrollments.Sum(e =>
            paidByEnrollment.TryGetValue(e.Id, out var paid)
                ? paid
                : 0);

        var remainingAmount = Math.Max(
            expectedAmount - totalPaid,
            0);

        var activeEnrollments = enrollments.Count(e =>
            e.Status == "Active");

        var cancelledEnrollments = enrollments.Count(e =>
            e.Status == "Cancelled");

        var courseIds = enrollments
            .Select(e => e.CourseId)
            .Distinct()
            .ToList();

        var lessonCounts = await _context.CourseLessons
            .AsNoTracking()
            .Where(l =>
                l.IsActive &&
                l.CourseModule != null &&
                l.CourseModule.IsActive &&
                courseIds.Contains(l.CourseModule.CourseId))
            .GroupBy(l => l.CourseModule!.CourseId)
            .Select(g => new
            {
                CourseId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(
                x => x.CourseId,
                x => x.Count);

        var completedCounts = await _context.LessonProgresses
            .AsNoTracking()
            .Where(p =>
                p.IsCompleted &&
                enrollmentIds.Contains(p.EnrollmentId))
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new
            {
                EnrollmentId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(
                x => x.EnrollmentId,
                x => x.Count);

        var totalLessons = 0;
        var completedLessons = 0;

        foreach (var enrollment in enrollments)
        {
            lessonCounts.TryGetValue(
                enrollment.CourseId,
                out var lessons);

            completedCounts.TryGetValue(
                enrollment.Id,
                out var completed);

            totalLessons += lessons;
            completedLessons += completed;
        }

        var collectionPercent = expectedAmount <= 0
            ? 0
            : Math.Round(
                totalPaid * 100m / expectedAmount,
                2);

        var progressPercent = totalLessons <= 0
            ? 0
            : Math.Round(
                completedLessons * 100m / totalLessons,
                2);

        var courses = enrollments
            .GroupBy(e => new
            {
                e.CourseId,
                CourseTitle = e.Course!.Title
            })
            .Select(g =>
            {
                var rows = g.ToList();

                var expected = rows.Sum(e =>
                    e.Course!.Price);

                var paid = rows.Sum(e =>
                    paidByEnrollment.TryGetValue(
                        e.Id,
                        out var amount)
                        ? amount
                        : 0);

                var remaining = Math.Max(
                    expected - paid,
                    0);

                return new
                {
                    courseId = g.Key.CourseId,
                    courseTitle = g.Key.CourseTitle,
                    enrollmentCount = rows.Count,
                    expectedAmount = expected,
                    totalPaid = paid,
                    remainingAmount = remaining,
                    collectionPercent = expected <= 0
                        ? 0
                        : Math.Round(
                            paid * 100m / expected,
                            2)
                };
            })
            .OrderByDescending(x => x.enrollmentCount)
            .ToList();

        return Ok(new
        {
            filters = new
            {
                from,
                to,
                courseId,
                instructorId,
                deliveryType,
                status
            },

            summary = new
            {
                studentCount = enrollments
                    .Select(e => e.StudentId)
                    .Distinct()
                    .Count(),

                courseCount = enrollments
                    .Select(e => e.CourseId)
                    .Distinct()
                    .Count(),

                enrollmentCount = enrollments.Count,

                activeEnrollments,

                cancelledEnrollments,

                expectedAmount,

                totalPaid,

                remainingAmount,

                collectionPercent,

                totalLessons,

                completedLessons,

                progressPercent
            },

            courses
        });
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? courseId,
        [FromQuery] int? instructorId,
        [FromQuery] string? deliveryType,
        [FromQuery] string? status)
    {
        var query = _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.Instructor)
            .Where(e => e.Course != null)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.StartDate >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.StartDate < to.Value.Date.AddDays(1));

        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);

        if (instructorId.HasValue)
            query = query.Where(e => e.InstructorId == instructorId.Value);

        if (!string.IsNullOrWhiteSpace(deliveryType))
            query = query.Where(e => e.Course!.DeliveryType == deliveryType);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e => e.Status == status);

        var enrollments = await query.ToListAsync();

        var enrollmentIds = enrollments.Select(e => e.Id).ToList();

        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p =>
                enrollmentIds.Contains(p.EnrollmentId) &&
                p.Status == "Paid")
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new
            {
                EnrollmentId = g.Key,
                Paid = g.Sum(p => p.Amount)
            })
            .ToDictionaryAsync(x => x.EnrollmentId, x => x.Paid);

        var courseIds = enrollments
            .Select(e => e.CourseId)
            .Distinct()
            .ToList();

        var lessonCounts = await _context.CourseLessons
            .AsNoTracking()
            .Where(l =>
                l.IsActive &&
                l.CourseModule != null &&
                l.CourseModule.IsActive &&
                courseIds.Contains(l.CourseModule.CourseId))
            .GroupBy(l => l.CourseModule!.CourseId)
            .Select(g => new
            {
                CourseId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var completedCounts = await _context.LessonProgresses
            .AsNoTracking()
            .Where(p =>
                p.IsCompleted &&
                enrollmentIds.Contains(p.EnrollmentId))
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new
            {
                EnrollmentId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.EnrollmentId, x => x.Count);

        var result = enrollments.Select(e =>
        {
            lessonCounts.TryGetValue(e.CourseId, out var totalLessons);
            completedCounts.TryGetValue(e.Id, out var completedLessons);
            payments.TryGetValue(e.Id, out var paid);

            var expected = e.Course?.Price ?? 0;
            var remaining = Math.Max(expected - paid, 0);

            return new
            {
                enrollmentId = e.Id,

                studentId = e.StudentId,

                studentName = e.Student == null
                    ? ""
                    : $"{e.Student.FirstName} {e.Student.LastName}".Trim(),

                courseId = e.CourseId,

                courseTitle = e.Course?.Title ?? "",

                instructorName = e.Instructor?.FullName ?? "",

                deliveryType = e.Course?.DeliveryType ?? "",

                startDate = e.StartDate,

                status = e.Status,

                expectedAmount = expected,

                totalPaid = paid,

                remainingAmount = remaining,

                totalLessons,

                completedLessons,

                progressPercent = totalLessons <= 0
                    ? 0
                    : Math.Round(
                        completedLessons * 100m / totalLessons,
                        2)
            };
        })
        .OrderByDescending(x => x.startDate)
        .ToList();

        return Ok(result);
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetTrend(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var start = from?.Date ?? DateTime.Today.AddMonths(-11);
        var end = (to?.Date ?? DateTime.Today).AddDays(1);

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e =>
                e.StartDate >= start &&
                e.StartDate < end)
            .Select(e => new
            {
                e.StartDate,
                e.CourseId
            })
            .ToListAsync();

        var enrollmentIds = await _context.Enrollments
            .AsNoTracking()
            .Where(e =>
                e.StartDate >= start &&
                e.StartDate < end)
            .Select(e => e.Id)
            .ToListAsync();

        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p =>
                enrollmentIds.Contains(p.EnrollmentId) &&
                p.Status == "Paid" &&
                p.PaymentDate >= start &&
                p.PaymentDate < end)
            .Select(p => new
            {
                p.PaymentDate,
                p.Amount
            })
            .ToListAsync();

        var enrollmentTrend = enrollments
            .GroupBy(e => new
            {
                e.StartDate.Year,
                e.StartDate.Month
            })
            .Select(g => new
            {
                year = g.Key.Year,
                month = g.Key.Month,
                enrollments = g.Count()
            })
            .ToList();

        var paymentTrend = payments
            .GroupBy(p => new
            {
                p.PaymentDate.Year,
                p.PaymentDate.Month
            })
            .Select(g => new
            {
                year = g.Key.Year,
                month = g.Key.Month,
                paid = g.Sum(x => x.Amount)
            })
            .ToList();

        var months = Enumerable.Range(
                0,
                ((end.Year - start.Year) * 12)
                + end.Month - start.Month + 1)
            .Select(i => start.AddMonths(i))
            .Where(d => d < end)
            .Select(d => new
            {
                year = d.Year,
                month = d.Month,
                label = $"{d.Year}/{d.Month:00}",
                enrollments = enrollmentTrend
                    .FirstOrDefault(x =>
                        x.year == d.Year &&
                        x.month == d.Month)
                    ?.enrollments ?? 0,
                paid = paymentTrend
                    .FirstOrDefault(x =>
                        x.year == d.Year &&
                        x.month == d.Month)
                    ?.paid ?? 0
            })
            .ToList();

        return Ok(months);
    }
}
