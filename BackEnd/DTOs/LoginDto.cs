using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    public string Password { get; set; } = string.Empty;
}