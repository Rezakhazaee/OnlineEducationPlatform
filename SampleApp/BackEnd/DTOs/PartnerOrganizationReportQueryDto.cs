namespace BackEnd.DTOs;

public class PartnerOrganizationReportQueryDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }

    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public DateTime? ContractStartFrom { get; set; }
    public DateTime? ContractStartTo { get; set; }

    public DateTime? ContractEndFrom { get; set; }
    public DateTime? ContractEndTo { get; set; }

    public string SortBy { get; set; } = "createdDate";
    public bool SortDescending { get; set; } = true;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
