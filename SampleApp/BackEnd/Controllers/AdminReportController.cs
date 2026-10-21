using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/AdminReport")]
[Authorize(Roles = "Admin")]
public class AdminReportController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminReportController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? courseId = null,
        [FromQuery] int? instructorId = null,
        [FromQuery] string? deliveryType = null,
        [FromQuery] string? status = null)
    {
        var query = _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Include(e => e.Student)
            .Include(e => e.Instructor)
            .Include(e => e.CoursePartnerOrganization)
            .Where(e => e.Course != null)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(e => e.StartDate >= from.Value);
        }

        if (to.HasValue)
        {
            var endDate = to.Value.Date.AddDays(1);
            query = query.Where(e => e.StartDate < endDate);
        }

        if (courseId.HasValue)
        {
            query = query.Where(e => e.CourseId == courseId.Value);
        }

        if (instructorId.HasValue)
        {
            query = query.Where(e => e.InstructorId == instructorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(deliveryType))
        {
            query = query.Where(
                e => e.Course!.DeliveryType == deliveryType);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(e => e.Status == status);
        }

        var enrollments = await query
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

        var paidByEnrollment = payments
            .Where(p => p.Status == "Paid")
            .GroupBy(p => p.EnrollmentId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(p => p.Amount));

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

        decimal expectedAmount = 0;
        decimal totalPaid = 0;

        foreach (var enrollment in enrollments)
        {
            var expected =
                enrollment.CoursePartnerOrganization?.AgreedPrice
                ?? enrollment.Course?.Price
                ?? 0;

            expectedAmount += expected;

            if (paidByEnrollment.TryGetValue(
                    enrollment.Id,
                    out var paid))
            {
                totalPaid += paid;
            }
        }

        var totalPending = payments
            .Where(p => p.Status == "Pending")
            .Sum(p => p.Amount);

        var totalCancelled = payments
            .Where(p => p.Status == "Cancelled")
            .Sum(p => p.Amount);

        var remainingAmount = Math.Max(
            expectedAmount - totalPaid,
            0);

        var activeEnrollments = enrollments
            .Count(e => e.Status == "Active");

        var cancelledEnrollments = enrollments
            .Count(e => e.Status == "Cancelled");

        var collectionPercent = expectedAmount <= 0
            ? 0
            : Math.Round(
                totalPaid * 100m / expectedAmount,
                2);

        var remainingPercent = expectedAmount <= 0
            ? 0
            : Math.Round(
                remainingAmount * 100m / expectedAmount,
                2);

        decimal progressTotal = 0;
        var progressRows = 0;

        var completedEnrollmentCount = 0;
        var inProgressEnrollmentCount = 0;
        var notStartedEnrollmentCount = 0;

        foreach (var enrollment in enrollments)
        {
            lessonCounts.TryGetValue(
                enrollment.CourseId,
                out var totalLessons);

            completedCounts.TryGetValue(
                enrollment.Id,
                out var completedLessons);

            if (totalLessons <= 0)
            {
                notStartedEnrollmentCount++;
                continue;
            }

            var progress =
                completedLessons * 100m / totalLessons;

            progressTotal += progress;
            progressRows++;

            if (completedLessons >= totalLessons)
            {
                completedEnrollmentCount++;
            }
            else if (completedLessons > 0)
            {
                inProgressEnrollmentCount++;
            }
            else
            {
                notStartedEnrollmentCount++;
            }
        }

        var averageProgress = progressRows == 0
            ? 0
            : Math.Round(
                progressTotal / progressRows,
                2);

        var courseReports = enrollments
            .GroupBy(e => new
            {
                e.CourseId,
                CourseTitle = e.Course!.Title
            })
            .Select(group =>
            {
                var rows = group.ToList();

                var expected = rows.Sum(e =>
                    e.CoursePartnerOrganization?.AgreedPrice
                    ?? e.Course!.Price);

                var paid = rows.Sum(e =>
                    paidByEnrollment.TryGetValue(
                        e.Id,
                        out var value)
                        ? value
                        : 0);

                var remaining = Math.Max(
                    expected - paid,
                    0);

                var courseCompleted = 0;
                var courseInProgress = 0;
                var courseNotStarted = 0;

                foreach (var enrollment in rows)
                {
                    lessonCounts.TryGetValue(
                        enrollment.CourseId,
                        out var lessons);

                    completedCounts.TryGetValue(
                        enrollment.Id,
                        out var completed);

                    if (lessons <= 0 || completed == 0)
                    {
                        courseNotStarted++;
                    }
                    else if (completed >= lessons)
                    {
                        courseCompleted++;
                    }
                    else
                    {
                        courseInProgress++;
                    }
                }

                return new
                {
                    courseId = group.Key.CourseId,
                    courseTitle = group.Key.CourseTitle,
                    deliveryType = rows
                        .Select(e => e.Course!.DeliveryType)
                        .FirstOrDefault(),

                    enrollmentCount = rows.Count,

                    expectedAmount = expected,
                    totalPaid = paid,
                    remainingAmount = remaining,

                    collectionPercent = expected <= 0
                        ? 0
                        : Math.Round(
                            paid * 100m / expected,
                            2),

                    completedCount = courseCompleted,
                    inProgressCount = courseInProgress,
                    notStartedCount = courseNotStarted
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
                    out var totalLessons);

                completedCounts.TryGetValue(
                    e.Id,
                    out var completedLessons);

                var progress = totalLessons <= 0
                    ? 0
                    : Math.Round(
                        completedLessons * 100m / totalLessons,
                        2);

                var expected =
                    e.CoursePartnerOrganization?.AgreedPrice
                    ?? e.Course?.Price
                    ?? 0;

                var paid = paidByEnrollment.TryGetValue(
                    e.Id,
                    out var value)
                    ? value
                    : 0;

                var learningStatus =
                    totalLessons <= 0 || completedLessons == 0
                        ? "NotStarted"
                        : completedLessons >= totalLessons
                            ? "Completed"
                            : "InProgress";

                return new
                {
                    enrollmentId = e.Id,
                    studentId = e.StudentId,

                    studentName = e.Student == null
                        ? ""
                        : $"{e.Student.FirstName} {e.Student.LastName}".Trim(),

                    courseId = e.CourseId,
                    courseTitle = e.Course?.Title ?? "",

                    instructorName =
                        e.Instructor?.FullName ?? "",

                    deliveryType =
                        e.Course?.DeliveryType ?? "",

                    expectedAmount = expected,
                    totalPaid = paid,

                    remainingAmount = Math.Max(
                        expected - paid,
                        0),

                    totalLessons,
                    completedLessons,
                    progressPercent = progress,

                    learningStatus,

                    status = e.Status,
                    startDate = e.StartDate
                };
            })
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

                totalCourses = enrollments
                    .Select(e => e.CourseId)
                    .Distinct()
                    .Count(),

                totalEnrollments = enrollments.Count,

                activeEnrollments,
                cancelledEnrollments,

                expectedAmount,
                totalPaid,
                totalPending,
                totalCancelled,
                remainingAmount,

                collectionPercent,
                remainingPercent,

                averageProgress,

                completedEnrollmentCount,
                inProgressEnrollmentCount,
                notStartedEnrollmentCount
            },

            learning = new
            {
                totalLessons = enrollments.Sum(e =>
                    lessonCounts.TryGetValue(
                        e.CourseId,
                        out var lessons)
                        ? lessons
                        : 0),

                completedLessons = completedCounts
                    .Where(x => enrollmentIds.Contains(x.Key))
                    .Sum(x => x.Value),

                completedEnrollmentCount,
                inProgressEnrollmentCount,
                notStartedEnrollmentCount,

                averageProgress
            },

            courses = courseReports,

            students = studentReports
        });
    }
}
