using BackEnd.Data;
using BackEnd.DTOs;
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
    public async Task<ActionResult<ReportsSummaryDto>> GetSummary()
    {
        var result = new ReportsSummaryDto
        {
            TotalStudents = await _context.Students.CountAsync(),

            TotalCourses = await _context.Courses.CountAsync(),

            TotalEnrollments = await _context.Enrollments.CountAsync(),

            TotalPaid = await _context.Payments
                .Where(p => p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0,

            TotalPending = await _context.Payments
                .Where(p => p.Status == "Pending")
                .SumAsync(p => (decimal?)p.Amount) ?? 0,

            TotalCancelled = await _context.Payments
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
            .Include(e => e.Course)
            .Include(e => e.Student)
            .Include(e => e.Instructor)
            .Where(e => e.Course != null);

        if (from.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery
                .Where(e => e.StartDate >= from.Value);
        }

        if (to.HasValue)
        {
            var endDate = to.Value.Date.AddDays(1);

            enrollmentsQuery = enrollmentsQuery
                .Where(e => e.StartDate < endDate);
        }

        if (courseId.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery
                .Where(e => e.CourseId == courseId.Value);
        }

        if (instructorId.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery
                .Where(e =>
                    e.InstructorId == instructorId.Value ||
                    e.Course!.InstructorId == instructorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(deliveryType))
        {
            enrollmentsQuery = enrollmentsQuery
                .Where(e => e.Course!.DeliveryType == deliveryType);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            enrollmentsQuery = enrollmentsQuery
                .Where(e => e.Status == status);
        }

        var enrollments = await enrollmentsQuery.ToListAsync();

        var enrollmentIds = enrollments
            .Select(e => e.Id)
            .ToList();

        var courseIds = enrollments
            .Select(e => e.CourseId)
            .Distinct()
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
                TotalPaid = g.Sum(p => p.Amount)
            })
            .ToDictionaryAsync(
                x => x.EnrollmentId,
                x => x.TotalPaid);

        var expectedAmount = enrollments.Sum(e =>
            e.CoursePartnerOrganization != null
                ? e.CoursePartnerOrganization.AgreedPrice ?? e.Course!.Price
                : e.Course!.Price);

        var totalPaid = enrollments.Sum(e =>
            paidByEnrollment.TryGetValue(
                e.Id,
                out var paid)
                ? paid
                : 0);

        var remainingAmount = Math.Max(
            expectedAmount - totalPaid,
            0);

        var activeEnrollments = enrollments
            .Count(e => e.Status == "Active");

        var cancelledEnrollments = enrollments
            .Count(e => e.Status == "Cancelled");
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var todayEnrollments = enrollments.Count(e =>
            e.StartDate.Date == today);

        var monthEnrollments = enrollments.Count(e =>
            e.StartDate >= monthStart &&
            e.StartDate < monthStart.AddMonths(1));

        var onlineEnrollments = enrollments.Count(e =>
            e.Course!.DeliveryType == "Online");

        var inPersonEnrollments = enrollments.Count(e =>
            e.Course!.DeliveryType == "InPerson");

        var distinctStudents = enrollments
            .Select(e => e.StudentId)
            .Distinct()
            .Count();

        var activeCourses = courseIds.Count;

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

        var totalLessons = 0;
        var completedLessons = 0;

        foreach (var enrollment in enrollments)
        {
            lessonCounts.TryGetValue(
                enrollment.CourseId,
                out var courseLessonCount);

            completedCounts.TryGetValue(
                enrollment.Id,
                out var enrollmentCompletedCount);

            totalLessons += courseLessonCount;
            completedLessons += enrollmentCompletedCount;
        }

        var progressPercent = totalLessons == 0
            ? 0
            : Math.Round(
                completedLessons * 100m / totalLessons,
                2);

        var collectionPercent = expectedAmount == 0
            ? 0
            : Math.Round(
                totalPaid * 100m / expectedAmount,
                2);

        var remainingPercent = expectedAmount == 0
            ? 0
            : Math.Round(
                remainingAmount * 100m / expectedAmount,
                2);

        var courseReports = enrollments
            .GroupBy(e => new
            {
                e.CourseId,
                CourseTitle = e.Course!.Title,
                DeliveryType = e.Course.DeliveryType
            })
            .Select(g =>
            {
                var rows = g.ToList();

                var expected = rows.Sum(e =>
                    e.CoursePartnerOrganization != null
                        ? e.CoursePartnerOrganization.AgreedPrice ?? e.Course!.Price
                        : e.Course!.Price);

                var paid = rows.Sum(e =>
                    paidByEnrollment.TryGetValue(
                        e.Id,
                        out var value)
                        ? value
                        : 0);

                var remaining = Math.Max(
                    expected - paid,
                    0);

                return new
                {
                    courseId = g.Key.CourseId,
                    capacity = g.First().Course!.Capacity,
                    activeStudentCount = rows.Count(e => e.Status == "Active"),
                    courseTitle = g.Key.CourseTitle,
                    deliveryType = g.Key.DeliveryType,
                    enrollmentCount = rows.Count,
                    expectedAmount = expected,
                    totalPaid = paid,
                    remainingAmount = remaining,
                    collectionPercent = expected == 0
                        ? 0
                        : Math.Round(
                            paid * 100m / expected,
                            2)
                };
            })
            .OrderByDescending(x => x.enrollmentCount)
            .ThenBy(x => x.courseTitle)
            .ToList();

        var studentReports = enrollments
            .Select(e =>
            {
                lessonCounts.TryGetValue(
                    e.CourseId,
                    out var lessons);

                completedCounts.TryGetValue(
                    e.Id,
                    out var completed);

                var expected = e.CoursePartnerOrganization != null
                    ? e.CoursePartnerOrganization.AgreedPrice ?? e.Course!.Price
                    : e.Course!.Price;

                var paid = paidByEnrollment.TryGetValue(
                    e.Id,
                    out var payment)
                    ? payment
                    : 0;

                return new
                {
                    enrollmentId = e.Id,

                    studentId = e.StudentId,

                    studentName = e.Student == null
                        ? string.Empty
                        : $"{e.Student.FirstName} {e.Student.LastName}".Trim(),

                    courseId = e.CourseId,

                    courseTitle = e.Course!.Title,

                    instructorName = e.Instructor?.FullName
                        ?? e.Course.Instructor?.FullName,

                    deliveryType = e.Course.DeliveryType,

                    expectedAmount = expected,

                    totalPaid = paid,

                    remainingAmount = Math.Max(
                        expected - paid,
                        0),

                    totalLessons = lessons,

                    completedLessons = completed,

                    progressPercent = lessons == 0
                        ? 0
                        : Math.Round(
                            completed * 100m / lessons,
                            2),

                    status = e.Status,

                    startDate = e.StartDate
                };
            })
            .OrderByDescending(x => x.startDate)
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
                totalStudents = distinctStudents,
                totalCourses = activeCourses,
                totalEnrollments = enrollments.Count,

                activeEnrollments,
                cancelledEnrollments,

                todayEnrollments,
                monthEnrollments,
                onlineEnrollments,
                inPersonEnrollments,

                expectedAmount,
                totalPaid,
                remainingAmount,

                collectionPercent,
                remainingPercent,

                totalLessons,
                completedLessons,
                progressPercent
            },

            courses = courseReports,

            students = studentReports
        });
    }
}
