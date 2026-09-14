namespace BackEnd.DTOs;

public class CoursePartnerOrganizationDto
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int PartnerOrganizationId { get; set; }

    public string? ContractNumber { get; set; }

    public decimal? AgreedPrice { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }

    public string? Description { get; set; }

    public PartnerOrganizationDto? PartnerOrganization { get; set; }
}

public class PartnerOrganizationDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
