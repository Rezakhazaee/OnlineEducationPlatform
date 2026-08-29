using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class CreateUserDto
{
    [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
    [MinLength(3, ErrorMessage = "نام و نام خانوادگی باید حداقل ۳ کاراکتر باشد")]
    public string FullName { get; set; } = string.Empty;


    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [Phone(ErrorMessage = "شماره موبایل معتبر نیست")]
    public string Mobile { get; set; } = string.Empty;


    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [MinLength(3, ErrorMessage = "نام کاربری باید حداقل ۳ کاراکتر باشد")]
    public string Username { get; set; } = string.Empty;


    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد")]
    public string PasswordHash { get; set; } = string.Empty;


    [Required(ErrorMessage = "نقش کاربر الزامی است")]
    public string Role { get; set; } = string.Empty;


    public bool IsActive { get; set; } = true;
}