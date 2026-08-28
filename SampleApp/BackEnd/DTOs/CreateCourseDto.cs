namespace BackEnd.DTOs;

public class CreateCourseDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? InstructorId { get; set; }

    public bool IsActive { get; set; } = true;
}