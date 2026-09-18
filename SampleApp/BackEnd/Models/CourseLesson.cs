namespace BackEnd.Models;

public class CourseLesson
{
    public int Id { get; set; }

    public int CourseModuleId { get; set; }

    public CourseModule? CourseModule { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Video / Text
    public string ContentType { get; set; } = "Video";

    // برای Video: آدرس ویدئو
    // برای Text: می‌تواند خالی باشد
    public string? ContentUrl { get; set; }

    public int DurationMinutes { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFreePreview { get; set; } = false;
}
