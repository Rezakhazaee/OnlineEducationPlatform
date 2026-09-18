namespace BackEnd.DTOs;

public class StudentCourseContentDto
{
    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public string DeliveryType { get; set; } = "Online";

    public List<StudentCourseModuleDto> Modules { get; set; } = new();
}

public class StudentCourseModuleDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public List<StudentCourseLessonDto> Lessons { get; set; } = new();
}

public class StudentCourseLessonDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ContentType { get; set; } = "Video";

    public string? ContentUrl { get; set; }

    public int DurationMinutes { get; set; }

    public int SortOrder { get; set; }

    public bool IsFreePreview { get; set; }
}
