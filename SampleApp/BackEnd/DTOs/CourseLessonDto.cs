namespace BackEnd.DTOs;

public class CourseLessonDto
{
    public int Id { get; set; }

    public int CourseModuleId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Video / Text
    public string ContentType { get; set; } = "Video";

    public string? ContentUrl { get; set; }

    public int DurationMinutes { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public bool IsFreePreview { get; set; }
}
