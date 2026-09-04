using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class ChangeUserPasswordDto
{
    [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد")]
    public string NewPassword { get; set; } = string.Empty;
}