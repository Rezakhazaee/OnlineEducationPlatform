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
        return Ok(await BuildReport());
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
        return Ok(await BuildReport(
            from,
            to,
            courseId,
            instructorId,
            deliveryType,
            status));
    }

    private async Task<object> BuildReport(
        DateTime? from = null,
        DateTime? to = null,
        int? courseId = null,
        int? instructorId = null,
        string? deliveryType = null,
        string? status = null)
    {
        var query = _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.Instructor)
            .Include(e => e.CoursePartnerOrganization)
            .Where(e => e.Course != null)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.StartDate >= from.Value.Date);

        if (to.HasValue)
        {
            var end = to.Value.Date.AddDays(1);
            query = query.Where(e => e.StartDate < end);
        }

        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);

        if (instructorId.HasValue)
            query = query.Where(e =>
                e.InstructorId == instructorId.Value ||
                e.Course!.InstructorId == instructorId.Value);

        if (!string.IsNullOrWhiteSpace(deliveryType))
            query = query.Where(e =>
                e.Course!.DeliveryType == deliveryType);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e => e.Status == status);

        var enrollments = await query.ToListAsync();

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

        decimal Expected(Models.Enrollment e) =>
            e.CoursePartnerOrganization?.AgreedPrice
            ?? e.Course!.Price;

        var totalExpected = enrollments.Sum(Expected);

        var totalPaid = payments
            .Where(p => p.Status == "Paid")
            .Sum(p => p.Amount);

        var totalPending = payments
            .Where(p => p.Status == "Pending")
            .Sum(p => p.Amount);

        var totalCancelled = payments
            .Where(p => p.Status == "Cancelled")
            .Sum(p => p.Amount);

        var progressRows = enrollments.Select(e =>
        {
            lessonCounts.TryGetValue(
                e.CourseId,
                out var totalLessons);

            completedCounts.TryGetValue(
                e.Id,
                out var completedLessons);

            var progress = totalLessons == 0
                ? 0
                : Math.Round(
                    completedLessons * 100m / totalLessons,
                    2);

            var state =
                totalLessons == 0 || completedLessons == 0
                    ? "NotStarted"
                    : completedLessons >= totalLessons
                        ? "Completed"
                        : "InProgress";

            return new
            {
                Id = e.Id,
                Progress = progress,
                State = state
            };
        }).ToList();

        var courseReports = enrollments
            .GroupBy(e => new
            {
                e.CourseId,
                CourseTitle = e.Course!.Title,
                e.Course.DeliveryType
            })
            .Select(g =>
            {
                var rows = g.ToList();
                var expected = rows.Sum(Expected);

                var ids = rows
                    .Select(x => x.Id)
                    .ToHashSet();

                var paid = payments
                    .Where(p =>
                        ids.Contains(p.EnrollmentId) &&
                        p.Status == "Paid")
                    .Sum(p => p.Amount);

                var averageProgress = progressRows
                    .Where(p => ids.Contains(p.Id))
                    .Select(p => p.Progress)
                    .DefaultIfEmpty()
                    .Average();

                return new
                {
                    courseId = g.Key.CourseId,
                    courseTitle = g.Key.CourseTitle,
                    deliveryType = g.Key.DeliveryType,
                    enrollmentCount = rows.Count,
                    expectedAmount = expected,
                    totalPaid = paid,
                    remainingAmount = Math.Max(
                        expected - paid,
                        0),
                    collectionPercent = expected == 0
                        ? 0
                        : Math.Round(
                            paid * 100m / expected,
                            2),
                    averageProgressPercent =
                        Math.Round(averageProgress, 2)
                };
            })
            .OrderByDescending(x => x.enrollmentCount)
            .ThenBy(x => x.courseTitle)
            .ToList();

        return new
        {
            totalStudents = enrollments
                .Select(e => e.StudentId)
                .Distinct()
                .Count(),

            totalCourses = courseReports.Count,

            totalEnrollments = enrollments.Count,

            totalExpected,

            totalPaid,

            totalRemaining =
                Math.Max(totalExpected - totalPaid, 0),

            totalPending,

            totalCancelled,

            activeEnrollments =
                enrollments.Count(e => e.Status == "Active"),

            cancelledEnrollments =
                enrollments.Count(e => e.Status == "Cancelled"),

            collectionPercent = totalExpected == 0
                ? 0
                : Math.Round(
                    totalPaid * 100m / totalExpected,
                    2),

            averageProgressPercent =
                progressRows.Count == 0
                    ? 0
                    : Math.Round(
                        progressRows.Average(x => x.Progress),
                        2),

            completedCount =
                progressRows.Count(
                    x => x.State == "Completed"),

            inProgressCount =
                progressRows.Count(
                    x => x.State == "InProgress"),

            notStartedCount =
                progressRows.Count(
                    x => x.State == "NotStarted"),

            courseReports,

            studentReports = enrollments
                .Select(e =>
                {
                    lessonCounts.TryGetValue(
                        e.CourseId,
                        out var totalLessons);

                    completedCounts.TryGetValue(
                        e.Id,
                        out var completedLessons);

                    var expected = Expected(e);

                    var paid = payments
                        .Where(p =>
                            p.EnrollmentId == e.Id &&
                            p.Status == "Paid")
                        .Sum(p => p.Amount);

                    var progress = totalLessons == 0
                        ? 0
                        : Math.Round(
                            completedLessons * 100m / totalLessons,
                            2);

                    return new
                    {
                        enrollmentId = e.Id,
                        studentId = e.StudentId,

                        studentName =
                            $"{e.Student?.FirstName} {e.Student?.LastName}"
                                .Trim(),

                        courseTitle = e.Course!.Title,

                        instructorName =
                            e.Instructor?.FullName
                            ?? e.Course.Instructor?.FullName,

                        status = e.Status,

                        expectedAmount = expected,

                        totalPaid = paid,

                        remainingAmount =
                            Math.Max(expected - paid, 0),

                        progressPercent = progress,

                        totalLessons,

                        completedLessons,

                        startDate = e.StartDate
                    };
                })
                .OrderByDescending(x => x.startDate)
                .ToList()
        };
    }
}
