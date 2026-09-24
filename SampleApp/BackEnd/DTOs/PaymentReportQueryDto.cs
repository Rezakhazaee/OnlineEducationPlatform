namespace BackEnd.DTOs;

public class PaymentReportQueryDto
{
    public string? Search { get; set; }
    public int? SupportUserId { get; set; }
    public int? MarketingUserId { get; set; }
    public int? InstructorId { get; set; }
    public int? PartnerOrganizationId { get; set; }

    public string? Status { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentMethod { get; set; }
    public string? EnrollmentStatus { get; set; }

    public DateTime? PaymentFrom { get; set; }
    public DateTime? PaymentTo { get; set; }

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public string SortBy { get; set; } = "paymentDate";
    public bool SortDescending { get; set; } = true;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
