using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class CreateEnrollmentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "شناسه دانشجو الزامی و باید معتبر باشد")]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "شناسه دوره الزامی و باید معتبر باشد")]
    public int CourseId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "شناسه پشتیبان باید معتبر باشد")]
    public int? SupportUserId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "شناسه استاد باید معتبر باشد")]
    public int? InstructorId { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "وضعیت ثبت نام الزامی است")]
    public string Status { get; set; } = "Active";
}