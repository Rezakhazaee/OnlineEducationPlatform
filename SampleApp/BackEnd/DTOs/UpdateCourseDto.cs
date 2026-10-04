using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class UpdateCourseDto
{
    [Required(ErrorMessage = "عنوان دوره الزامی است")]
    [MinLength(3, ErrorMessage = "عنوان دوره باید حداقل ۳ کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد")]
    public decimal Price { get; set; }

    public bool IsActive { get; set; }
}