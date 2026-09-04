namespace BackEnd.DTOs;

public class InstructorDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int CourseCount { get; set; }
}