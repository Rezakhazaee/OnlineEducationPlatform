namespace BackEnd.Models;

public class Enrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public Student? Student { get; set; }

    public int CourseId { get; set; }

    public Course? Course { get; set; }

    public int? SupportUserId { get; set; }

    public User? SupportUser { get; set; }

    public int? InstructorId { get; set; }

    public User? Instructor { get; set; }

    public DateTime StartDate { get; set; }

    public string Status { get; set; } = "Active";

    // توضیحات مربوط به ثبت‌نام دانشجو در دوره
    public string? Description { get; set; }
}