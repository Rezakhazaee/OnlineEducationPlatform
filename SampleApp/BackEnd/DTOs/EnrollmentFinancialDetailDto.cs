namespace BackEnd.DTOs;

public class EnrollmentFinancialDetailDto
{
    public int EnrollmentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public decimal CoursePrice { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal RemainingAmount { get; set; }

    public string PaymentStatus { get; set; } = string.Empty;

    public List<PaymentItemDto> Payments { get; set; } = new();
}