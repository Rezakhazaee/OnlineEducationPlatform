namespace BackEnd.DTOs;

public class PaymentReportItemDto
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string PaymentType { get; set; } = string.Empty;

    public string? PaymentMethod { get; set; }

    public string? GatewayRefId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string EnrollmentStatus { get; set; } = string.Empty;

    public string? SupportUserName { get; set; }

    public string? MarketingUserName { get; set; }

    public string? InstructorName { get; set; }

    public string? PartnerOrganizationName { get; set; }

    public string? Description { get; set; }
}
