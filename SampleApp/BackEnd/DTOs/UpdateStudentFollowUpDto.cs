using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class UpdateStudentFollowUpDto
{
    public DateTime FollowUpDate { get; set; }

    [Required(ErrorMessage = "وضعیت پیگیری الزامی است")]
    public string Status { get; set; } = "Pending";

    public string? Description { get; set; }
}