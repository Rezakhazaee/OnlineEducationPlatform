namespace BackEnd.DTOs;

public class EnrollmentReportItemDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;

    public decimal CoursePrice { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;

    public int? SupportUserId { get; set; }
    public string? SupportUserName { get; set; }

    public int? InstructorId { get; set; }
    public string? InstructorName { get; set; }

    public int? MarketingUserId { get; set; }
    public string? MarketingUserName { get; set; }

    public int? PartnerOrganizationId { get; set; }
    public string? PartnerOrganizationName { get; set; }

    public DateTime StartDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
}
