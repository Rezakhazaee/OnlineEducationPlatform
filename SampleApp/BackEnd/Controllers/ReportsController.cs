using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = new
        {
            totalStudents = await _context.Students.CountAsync(),
            totalCourses = await _context.Courses.CountAsync(),
            totalEnrollments = await _context.Enrollments.CountAsync(),

            totalPaid = await _context.Payments
                .Where(p => p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0,

            totalPending = await _context.Payments
                .Where(p => p.Status == "Pending")
                .SumAsync(p => (decimal?)p.Amount) ?? 0,

            totalCancelled = await _context.Payments
                .Where(p => p.Status == "Cancelled")
                .SumAsync(p => (decimal?)p.Amount) ?? 0
        };

        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? courseId,
        [FromQuery] int? instructorId,
        [FromQuery] string? deliveryType,
        [FromQuery] string? status)
    {
        var enrollmentsQuery = _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Instructor)
            .Include(e => e.Instructor)
            .Include(e => e.CoursePartnerOrganization)
            .AsQueryable();

        if (from.HasValue)
        {
            var fromDate = from.Value.Date;
            enrollmentsQuery =
                enrollmentsQuery.Where(e => e.StartDate >= fromDate);
        }

        if (to.HasValue)
        {
            var toDate = to.Value.Date.AddDays(1);
            enrollmentsQuery =
                enrollmentsQuery.Where(e => e.StartDate < toDate);
        }

        if (courseId.HasValue)
        {
            enrollmentsQuery =
                enrollmentsQuery.Where(e => e.CourseId == courseId.Value);
        }

        if (instructorId.HasValue)
        {
            enrollmentsQuery =
                enrollmentsQuery.Where(e =>
                    e.Course != null &&
                    e.Course.InstructorId == instructorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(deliveryType))
        {
            enrollmentsQuery =
                enrollmentsQuery.Where(e =>
                    e.Course != null &&
                    e.Course.DeliveryType == deliveryType);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            enrollmentsQuery =
                enrollmentsQuery.Where(e => e.Status == status);
        }

        var enrollments = await enrollmentsQuery
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();

        var enrollmentIds = enrollments
            .Select(e => e.Id)
            .ToList();

        var courseIds = enrollments
            .Select(e => e.CourseId)
            .Distinct()
            .ToList();

        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => enrollmentIds.Contains(p.EnrollmentId))
            .ToListAsync();

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
                TotalLessons = g.Count()
            })
            .ToDictionaryAsync(
                x => x.CourseId,
                x => x.TotalLessons);

        var completedCounts = await _context.LessonProgresses
            .AsNoTracking()
            .Where(p =>
                p.IsCompleted &&
                enrollmentIds.Contains(p.EnrollmentId))
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new
            {
                EnrollmentId = g.Key,
                CompletedLessons = g.Count()
            })
            .ToDictionaryAsync(
                x => x.EnrollmentId,
                x => x.CompletedLessons);

        var expectedAmount = enrollments.Sum(e =>
            e.CoursePartnerOrganization?.AgreedPrice
            ?? e.Course?.Price
            ?? 0);

        var totalPaid = payments
            .Where(p => p.Status == "Paid")
            .Sum(p => p.Amount);

        var totalPending = payments
            .Where(p => p.Status == "Pending")
            .Sum(p => p.Amount);

        var totalCancelled = payments
            .Where(p => p.Status == "Cancelled")
            .Sum(p => p.Amount);

        var remainingAmount =
            Math.Max(expectedAmount - totalPaid, 0);

        var collectionPercent =
            expectedAmount <= 0
                ? 0
                : Math.Round(
                    totalPaid * 100m / expectedAmount,
                    2);

        var totalLessons =
            enrollments.Sum(e =>
                lessonCounts.TryGetValue(
                    e.CourseId,
                    out var count)
                    ? count
                    : 0);

        var completedLessons =
            enrollments.Sum(e =>
                completedCounts.TryGetValue(
                    e.Id,
                    out var count)
                    ? count
                    : 0);

        var progressPercent =
            totalLessons <= 0
                ? 0
                : Math.Round(
                    completedLessons * 100m / totalLessons,
                    2);

        var activeEnrollments =
            enrollments.Count(e => e.Status == "Active");

        var cancelledEnrollments =
            enrollments.Count(e => e.Status == "Cancelled");

        var inProgressEnrollments =
            enrollments.Count(e =>
            {
                var total =
                    lessonCounts.TryGetValue(
                        e.CourseId,
                        out var totalCount)
                        ? totalCount
                        : 0;

                var completed =
                    completedCounts.TryGetValue(
                        e.Id,
                        out var completedCount)
                        ? completedCount
                        : 0;

                return total > 0 &&
                       completed > 0 &&
                       completed < total;
            });

        var completedEnrollments =
            enrollments.Count(e =>
            {
                var total =
                    lessonCounts.TryGetValue(
                        e.CourseId,
                        out var totalCount)
                        ? totalCount
                        : 0;

                var completed =
                    completedCounts.TryGetValue(
                        e.Id,
                        out var completedCount)
                        ? completedCount
                        : 0;

                return total > 0 &&
                       completed >= total;
            });

        var notStartedEnrollments =
            enrollments.Count(e =>
            {
                var completed =
                    completedCounts.TryGetValue(
                        e.Id,
                        out var completedCount)
                        ? completedCount
                        : 0;

                return completed == 0;
            });

        var courseReports = enrollments
            .Where(e => e.Course != null)
            .GroupBy(e => new
            {
                e.CourseId,
                CourseTitle = e.Course!.Title,
                e.Course.DeliveryType,
                e.Course.InstructorId
            })
            .Select(group =>
            {
                var rows = group.ToList();

                var expected = rows.Sum(e =>
                    e.CoursePartnerOrganization?.AgreedPrice
                    ?? e.Course!.Price);

                var paid = rows
                    .SelectMany(e =>
                        payments.Where(p =>
                            p.EnrollmentId == e.Id &&
                            p.Status == "Paid"))
                    .Sum(p => p.Amount);

                var courseRemaining =
                    Math.Max(expected - paid, 0);

                var courseCollection =
                    expected <= 0
                        ? 0
                        : Math.Round(
                            paid * 100m / expected,
                            2);

                var lessons = rows.Sum(e =>
                    lessonCounts.TryGetValue(
                        e.CourseId,
                        out var count)
                        ? count
                        : 0);

                var completed = rows.Sum(e =>
                    completedCounts.TryGetValue(
                        e.Id,
                        out var count)
                        ? count
                        : 0);

                var progress =
                    lessons <= 0
                        ? 0
                        : Math.Round(
                            completed * 100m / lessons,
                            2);

                return new
                {
                    courseId = group.Key.CourseId,
                    courseTitle = group.Key.CourseTitle,
                    deliveryType = group.Key.DeliveryType,
                    instructorId = group.Key.InstructorId,
                    enrollmentCount = rows.Count,
                    expectedAmount = expected,
                    totalPaid = paid,
                    remainingAmount = courseRemaining,
                    collectionPercent = courseCollection,
                    progressPercent = progress
                };
            })
            .OrderByDescending(x => x.enrollmentCount)
            .ThenBy(x => x.courseTitle)
            .ToList();

        var studentReports = enrollments
            .Where(e => e.Student != null && e.Course != null)
            .Select(e =>
            {
                var total =
                    lessonCounts.TryGetValue(
                        e.CourseId,
                        out var totalCount)
                        ? totalCount
                        : 0;

                var completed =
                    completedCounts.TryGetValue(
                        e.Id,
                        out var completedCount)
                        ? completedCount
                        : 0;

                var progress =
                    total <= 0
                        ? 0
                        : Math.Round(
                            completed * 100m / total,
                            2);

                var expected =
                    e.CoursePartnerOrganization?.AgreedPrice
                    ?? e.Course!.Price;

                var paid = payments
                    .Where(p =>
                        p.EnrollmentId == e.Id &&
                        p.Status == "Paid")
                    .Sum(p => p.Amount);

                return new
                {
                    enrollmentId = e.Id,

                    studentId = e.StudentId,

                    studentName =
                        $"{e.Student!.FirstName} {e.Student.LastName}".Trim(),

                    courseId = e.CourseId,

                    courseTitle = e.Course!.Title,

                    instructorName =
                        e.Instructor?.FullName
                        ?? e.Course.Instructor?.FullName,

                    deliveryType = e.Course.DeliveryType,

                    startDate = e.StartDate,

                    status = e.Status,

                    totalLessons = total,

                    completedLessons = completed,

                    progressPercent = progress,

                    expectedAmount = expected,

                    totalPaid = paid,

                    remainingAmount =
                        Math.Max(expected - paid, 0)
                };
            })
            .ToList();

        var monthlyReports = enrollments
            .GroupBy(e => new
            {
                e.StartDate.Year,
                e.StartDate.Month
            })
            .Select(g =>
            {
                var ids = g.Select(e => e.Id).ToHashSet();

                var monthPaid = payments
                    .Where(p =>
                        ids.Contains(p.EnrollmentId) &&
                        p.Status == "Paid")
                    .Sum(p => p.Amount);

                return new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    enrollmentCount = g.Count(),
                    paidAmount = monthPaid
                };
            })
            .OrderBy(x => x.year)
            .ThenBy(x => x.month)
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
                totalStudents = enrollments
                    .Select(e => e.StudentId)
                    .Distinct()
                    .Count(),

                totalCourses = courseIds.Count,

                totalEnrollments = enrollments.Count,

                activeEnrollments,

                cancelledEnrollments,

                expectedAmount,

                totalPaid,

                totalPending,

                totalCancelled,

                remainingAmount,

                collectionPercent,

                totalLessons,

                completedLessons,

                progressPercent,

                completedEnrollments,

                inProgressEnrollments,

                notStartedEnrollments
            },

            courseReports,

            studentReports,

            monthlyReports
        });
    }
}
