namespace BackEnd.Models;

public class EnrollmentLessonProgress
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public Enrollment? Enrollment { get; set; }

    public int CourseLessonId { get; set; }

    public CourseLesson? CourseLesson { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }
}
