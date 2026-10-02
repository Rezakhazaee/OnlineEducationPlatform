using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/AdminProgress")]
[Authorize(Roles = "Admin")]
public class AdminProgressController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminProgressController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.Instructor)
            .Where(e => e.Status != "Cancelled")
            .OrderBy(e => e.Course!.Title)
            .ThenBy(e => e.Student!.FirstName)
            .ThenBy(e => e.Student!.LastName)
            .ToListAsync();

        var courseIds = enrollments
            .Select(e => e.CourseId)
            .Distinct()
            .ToList();

        var enrollmentIds = enrollments
            .Select(e => e.Id)
            .ToList();

        var lessonCounts = await _context.CourseLessons
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

        var result = enrollments.Select(e =>
        {
            lessonCounts.TryGetValue(
                e.CourseId,
                out var totalLessons);

            completedCounts.TryGetValue(
                e.Id,
                out var completedLessons);

            return new
            {
                enrollmentId = e.Id,

                studentName = e.Student == null
                    ? string.Empty
                    : $"{e.Student.FirstName} {e.Student.LastName}".Trim(),

                courseTitle = e.Course?.Title ?? string.Empty,

                instructorName = e.Instructor?.FullName,

                totalLessons,
                completedLessons,

                progressPercent = totalLessons == 0
                    ? 0
                    : Math.Round(
                        completedLessons * 100m / totalLessons,
                        2),

                startDate = e.StartDate,
                status = e.Status
            };
        }).ToList();

        return Ok(result);
    }
}
