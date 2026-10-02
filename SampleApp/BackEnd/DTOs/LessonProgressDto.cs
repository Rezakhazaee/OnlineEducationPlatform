namespace BackEnd.DTOs;

public class LessonProgressItemDto
{
    public int LessonId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class LessonProgressSummaryDto
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public decimal ProgressPercent { get; set; }
    public List<LessonProgressItemDto> Lessons { get; set; } = new();
}

public class UpdateLessonProgressDto
{
    public bool Completed { get; set; }
}
