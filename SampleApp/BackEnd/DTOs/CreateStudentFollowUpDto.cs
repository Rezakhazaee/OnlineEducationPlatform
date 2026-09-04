using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class CreateStudentFollowUpDto
{
    [Required(ErrorMessage = "دانشجو الزامی است")]
    public int StudentId { get; set; }

    public DateTime FollowUpDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "وضعیت پیگیری الزامی است")]
    public string Status { get; set; } = "Pending";

    public string? Description { get; set; }
}