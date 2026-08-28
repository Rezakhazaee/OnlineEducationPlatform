namespace BackEnd.Models;

public class Enrollment
{
    public int Id { get; set; }


    // دانشجو
    public int StudentId { get; set; }

    public Student? Student { get; set; }


    // دوره
    public int CourseId { get; set; }

    public Course? Course { get; set; }


    // پشتیبان آموزشی
    public int? SupportUserId { get; set; }

    public User? SupportUser { get; set; }


    // استاد دوره
    public int? InstructorId { get; set; }

    public User? Instructor { get; set; }


    public DateTime StartDate { get; set; } = DateTime.Now;


    public string Status { get; set; } = "Active";
}