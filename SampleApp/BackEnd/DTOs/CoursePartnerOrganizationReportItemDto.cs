namespace BackEnd.DTOs;

public class CoursePartnerOrganizationReportItemDto
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public int PartnerOrganizationId { get; set; }

    public string PartnerOrganizationName { get; set; } = string.Empty;

    public string? ContractNumber { get; set; }

    public decimal? AgreedPrice { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }

    public string ContractStatus { get; set; } = string.Empty;

    public string? Description { get; set; }
}
