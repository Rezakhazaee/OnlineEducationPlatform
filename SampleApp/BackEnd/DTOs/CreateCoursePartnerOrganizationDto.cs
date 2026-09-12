using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class CreateCoursePartnerOrganizationDto
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    public int PartnerOrganizationId { get; set; }

    public string? ContractNumber { get; set; }

    public decimal? AgreedPrice { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
