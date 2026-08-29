using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class CreatePaymentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "شناسه ثبت نام الزامی و باید معتبر باشد")]
    public int EnrollmentId { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335",
        ErrorMessage = "مبلغ پرداخت باید بیشتر از صفر باشد")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "نوع پرداخت الزامی است")]
    [MinLength(3, ErrorMessage = "نوع پرداخت باید حداقل ۳ کاراکتر باشد")]
    public string PaymentType { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "وضعیت پرداخت الزامی است")]
    public string Status { get; set; } = "Paid";
}