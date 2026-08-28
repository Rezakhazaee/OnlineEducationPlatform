namespace BackEnd.DTOs;

public class EnrollmentDto
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public int? SupportUserId { get; set; }

    public int? InstructorId { get; set; }

    public DateTime StartDate { get; set; }

    public string Status { get; set; } = string.Empty;
}