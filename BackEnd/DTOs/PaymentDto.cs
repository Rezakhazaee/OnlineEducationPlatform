namespace BackEnd.DTOs;

public class PaymentDto
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string PaymentType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;
}