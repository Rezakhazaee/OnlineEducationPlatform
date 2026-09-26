using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class UpdatePaymentDueDateDto
{
    [Required(ErrorMessage = "تاریخ سررسید الزامی است")]
    public DateTime? DueDate { get; set; }
}
