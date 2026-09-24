namespace BackEnd.DTOs;

public class StudentReportQueryDto
{
    public string? Search { get; set; }

    public int? SupportUserId { get; set; }

    public int? MarketingUserId { get; set; }

    public int? PartnerOrganizationId { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public string SortBy { get; set; } = "createdDate";

    public bool SortDescending { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
