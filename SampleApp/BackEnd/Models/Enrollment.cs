namespace BackEnd.Models;

public class Enrollment
{
    public int Id { get; set; }


    public int StudentId { get; set; }


    public int CourseId { get; set; }


    // پشتیبان آموزشی (اختیاری)
    public int? SupportUserId { get; set; }


    // استاد دوره (اختیاری)
    public int? InstructorId { get; set; }


    public DateTime StartDate { get; set; } = DateTime.Now;


    public string Status { get; set; } = "Active";
}