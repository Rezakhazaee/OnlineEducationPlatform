namespace BackEnd.DTOs;

public class PaymentDetailDto
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string PaymentType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;
}