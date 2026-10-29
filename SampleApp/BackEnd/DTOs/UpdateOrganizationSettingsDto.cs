using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class UpdateOrganizationSettingsDto
{
    [Required(ErrorMessage = "نام سازمان الزامی است.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "نام سازمان باید حداکثر ۱۰۰ نویسه باشد.")]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(1000, ErrorMessage = "توضیحات سازمان باید حداکثر ۱۰۰۰ نویسه باشد.")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "رنگ اصلی الزامی است.")]
    [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "رنگ اصلی باید به قالب #RRGGBB باشد.")]
    public string PrimaryColor { get; set; } = "#568fa8";

    [Required(ErrorMessage = "رنگ دوم الزامی است.")]
    [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "رنگ دوم باید به قالب #RRGGBB باشد.")]
    public string SecondaryColor { get; set; } = "#263d4a";

    public bool ThemeIsActive { get; set; } = true;

    // 1 = حضوری
    // 2 = آموزش آنلاین
    // 3 = پرداخت آنلاین
    // 4 = سازمانی
    [System.ComponentModel.DataAnnotations.Range(1, 4, ErrorMessage = "سطح بسته باید عددی بین ۱ تا ۴ باشد.")]
    public int PackageLevel { get; set; } = 1;
}
