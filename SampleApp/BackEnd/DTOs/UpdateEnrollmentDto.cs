using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class UpdateEnrollmentDto
{
    public int? CoursePartnerOrganizationId { get; set; }

    public int? SupportUserId { get; set; }

    public int? InstructorId { get; set; }

    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "وضعیت ثبت‌نام الزامی است")]
    public string Status { get; set; } = "Active";

    public string? Description { get; set; }
}