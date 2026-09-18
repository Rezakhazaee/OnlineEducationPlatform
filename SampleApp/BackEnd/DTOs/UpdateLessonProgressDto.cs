namespace BackEnd.DTOs;

public class UpdateLessonProgressDto
{
    public int EnrollmentId { get; set; }

    public int CourseLessonId { get; set; }

    public bool IsCompleted { get; set; }
}
