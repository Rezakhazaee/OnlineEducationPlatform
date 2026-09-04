using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class CreateEnrollmentDto
{
    [Required(ErrorMessage = "دانشجو الزامی است")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "دوره الزامی است")]
    public int CourseId { get; set; }

    public int? SupportUserId { get; set; }

    public int? InstructorId { get; set; }

    public DateTime StartDate { get; set; }

    public string Status { get; set; } = "Active";

    // توضیحات ثبت‌نام
    public string? Description { get; set; }
}