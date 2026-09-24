namespace BackEnd.DTOs;

public class PartnerOrganizationReportItemDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string? ContactMobile { get; set; }

    public string? ContractNumber { get; set; }

    public DateTime? ContractStartDate { get; set; }

    public DateTime? ContractEndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CourseCount { get; set; }

    public int StudentCount { get; set; }

    public string ContractStatus { get; set; } = string.Empty;
}
