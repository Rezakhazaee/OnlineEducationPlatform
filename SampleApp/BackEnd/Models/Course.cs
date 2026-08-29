namespace BackEnd.Models;

public class Course
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    // استاد دوره
    public int? InstructorId { get; set; }

    public User? Instructor { get; set; }

    public bool IsActive { get; set; } = true;
}