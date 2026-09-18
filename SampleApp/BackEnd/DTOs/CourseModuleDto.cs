namespace BackEnd.DTOs;

public class CourseModuleDto
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public int LessonCount { get; set; }
}
