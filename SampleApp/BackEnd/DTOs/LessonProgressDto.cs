namespace BackEnd.DTOs;

public class LessonProgressDto
{
    public int CourseLessonId { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }
}
