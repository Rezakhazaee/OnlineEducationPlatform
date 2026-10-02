namespace BackEnd.Models;

public class CoursePartnerOrganization
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course? Course { get; set; }

    public int PartnerOrganizationId { get; set; }

    public PartnerOrganization? PartnerOrganization { get; set; }

    public string? ContractNumber { get; set; }

    public decimal? AgreedPrice { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
