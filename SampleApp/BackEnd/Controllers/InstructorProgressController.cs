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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var instructorId))
            return Unauthorized();

        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Id == courseId &&
                c.InstructorId == instructorId);

        if (course == null)
        {
            return NotFound(new
            {
                message = "دوره مورد نظر برای این مدرس پیدا نشد."
            });
        }

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

        var enrollmentIds = enrollments
            .Select(e => e.Id)
            .ToList();

        var completedByEnrollment = await _context.LessonProgresses
            .Where(p =>
                enrollmentIds.Contains(p.EnrollmentId) &&
                p.IsCompleted &&
                lessonIds.Contains(p.CourseLessonId))
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
            completedByEnrollment.TryGetValue(
                e.Id,
                out var completed);

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
                completedLessons = completed,
                progressPercent = total == 0
                    ? 0
                    : Math.Round(completed * 100m / total, 2)
            };
        }).ToList();

        return Ok(result);
    }
}
