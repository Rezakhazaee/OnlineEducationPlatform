using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class CreateStudentDto
{
    [Required(ErrorMessage = "نام دانشجو الزامی است")]
    [MinLength(2, ErrorMessage = "نام باید حداقل ۲ کاراکتر باشد")]
    public string FirstName { get; set; } = string.Empty;


    [Required(ErrorMessage = "نام خانوادگی دانشجو الزامی است")]
    [MinLength(2, ErrorMessage = "نام خانوادگی باید حداقل ۲ کاراکتر باشد")]
    public string LastName { get; set; } = string.Empty;


    [Required(ErrorMessage = "کد ملی الزامی است")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید دقیقاً ۱۰ رقم باشد")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید فقط شامل ۱۰ رقم باشد")]
    public string NationalCode { get; set; } = string.Empty;


    public DateTime? BirthDate { get; set; }


    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید با 09 شروع شود و ۱۱ رقم باشد")]
    public string Mobile { get; set; } = string.Empty;


    public string? Address { get; set; }


    public string? GuardianName { get; set; }


    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل سرپرست باید با 09 شروع شود و ۱۱ رقم باشد")]
    public string? GuardianMobile { get; set; }


    public int? OrganizationId { get; set; }


    public int? MarketingUserId { get; set; }


    public int? SupportUserId { get; set; }
}