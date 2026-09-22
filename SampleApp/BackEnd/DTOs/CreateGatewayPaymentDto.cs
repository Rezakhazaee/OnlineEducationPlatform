using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class CreateGatewayPaymentDto
{
    [Range(1, int.MaxValue)]
    public int EnrollmentId { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Amount { get; set; }

    [Required]
    public string PaymentType { get; set; } = string.Empty;
}
