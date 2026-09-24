namespace BackEnd.DTOs;

public class CoursePartnerOrganizationReportQueryDto
{
    public string? Search { get; set; }

    public int? CourseId { get; set; }

    public int? PartnerOrganizationId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? StartFrom { get; set; }

    public DateTime? StartTo { get; set; }

    public DateTime? EndFrom { get; set; }

    public DateTime? EndTo { get; set; }

    public decimal? MinAgreedPrice { get; set; }

    public decimal? MaxAgreedPrice { get; set; }

    public string SortBy { get; set; } = "id";

    public bool SortDescending { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
