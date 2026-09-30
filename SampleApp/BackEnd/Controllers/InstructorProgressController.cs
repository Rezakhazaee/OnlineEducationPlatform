using System.Security.Claims;
using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/InstructorProgress")]
[Authorize(Roles = "Instructor")]
public class InstructorProgressController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InstructorProgressController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetCourseStudents(int courseId)
    {
        var instructorId = GetInstructorId();
        if (instructorId == null) return Unauthorized();

        var courseExists = await _context.Courses.AnyAsync(c =>
            c.Id == courseId &&
            c.InstructorId == instructorId.Value);

        if (!courseExists)
            return NotFound(new { message = "دوره مورد نظر پیدا نشد." });

        var lessonIds = await _context.CourseLessons
            .Where(l =>
                l.IsActive &&
                l.CourseModule != null &&
                l.CourseModule.IsActive &&
                l.CourseModule.CourseId == courseId)
            .Select(l => l.Id)
            .ToListAsync();

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Where(e =>
                e.CourseId == courseId &&
                e.Status != "Cancelled")
            .OrderBy(e => e.Student!.FirstName)
            .ThenBy(e => e.Student!.LastName)
            .ToListAsync();

        var ids = enrollments.Select(e => e.Id).ToList();

        var completed = await _context.LessonProgresses
            .Where(p =>
                ids.Contains(p.EnrollmentId) &&
                p.IsCompleted &&
                lessonIds.Contains(p.CourseLessonId))
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        return Ok(enrollments.Select(e =>
        {
            completed.TryGetValue(e.Id, out var done);
            var total = lessonIds.Count;

            return new
            {
                enrollmentId = e.Id,
                studentId = e.StudentId,
                studentName = e.Student == null
                    ? string.Empty
                    : $"{e.Student.FirstName} {e.Student.LastName}".Trim(),
                mobile = e.Student?.Mobile,
                startDate = e.StartDate,
                status = e.Status,
                totalLessons = total,
                completedLessons = done,
                progressPercent = total == 0
                    ? 0
                    : Math.Round(done * 100m / total, 2)
            };
        }));
    }

    [HttpGet("enrollment/{enrollmentId:int}")]
    public async Task<IActionResult> GetStudentDetails(int enrollmentId)
    {
        var instructorId = GetInstructorId();
        if (instructorId == null) return Unauthorized();

        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null ||
            enrollment.Course == null ||
            enrollment.Course.InstructorId != instructorId.Value)
        {
            return NotFound(new { message = "ثبت‌نام مورد نظر پیدا نشد." });
        }

        var lessons = await _context.CourseLessons
            .AsNoTracking()
            .Where(l =>
                l.IsActive &&
                l.CourseModule != null &&
                l.CourseModule.IsActive &&
                l.CourseModule.CourseId == enrollment.CourseId)
            .OrderBy(l => l.CourseModule!.SortOrder)
            .ThenBy(l => l.SortOrder)
            .ThenBy(l => l.Id)
            .Select(l => new
            {
                lessonId = l.Id,
                title = l.Title,
                contentType = l.ContentType,
                durationMinutes = l.DurationMinutes
            })
            .ToListAsync();

        var lessonIds = lessons.Select(x => x.lessonId).ToList();

        var progress = await _context.LessonProgresses
            .AsNoTracking()
            .Where(p =>
                p.EnrollmentId == enrollmentId &&
                lessonIds.Contains(p.CourseLessonId))
            .ToDictionaryAsync(p => p.CourseLessonId);

        return Ok(new
        {
            enrollmentId = enrollment.Id,
            studentName = enrollment.Student == null
                ? string.Empty
                : $"{enrollment.Student.FirstName} {enrollment.Student.LastName}".Trim(),
            lessons = lessons.Select(l =>
            {
                progress.TryGetValue(l.lessonId, out var p);

                return new
                {
                    l.lessonId,
                    l.title,
                    l.contentType,
                    l.durationMinutes,
                    isCompleted = p?.IsCompleted ?? false,
                    completedAt = p?.CompletedAt
                };
            })
        });
    }

    private int? GetInstructorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
