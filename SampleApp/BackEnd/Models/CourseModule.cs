namespace BackEnd.Models;

public class CourseModule
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course? Course { get; set; }

    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
}
