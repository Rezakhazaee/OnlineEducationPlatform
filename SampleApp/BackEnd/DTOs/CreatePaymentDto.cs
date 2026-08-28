namespace BackEnd.DTOs;

public class CreatePaymentDto
{
    public int EnrollmentId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    public string PaymentType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = "Paid";
}