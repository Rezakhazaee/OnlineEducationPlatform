namespace BackEnd.DTOs;

public class EnrollmentDetailDto
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;


    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;


    public int? SupportUserId { get; set; }

    public string? SupportUserName { get; set; }


    public int? InstructorId { get; set; }

    public string? InstructorName { get; set; }


    public DateTime StartDate { get; set; }

    public string Status { get; set; } = string.Empty;
}