namespace BackEnd.DTOs;

public class EnrollmentReportQueryDto
{
    public string? Search { get; set; }
    public int? SupportUserId { get; set; }
    public int? InstructorId { get; set; }
    public int? MarketingUserId { get; set; }
    public int? PartnerOrganizationId { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? StartFrom { get; set; }
    public DateTime? StartTo { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string SortBy { get; set; } = "startDate";
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
