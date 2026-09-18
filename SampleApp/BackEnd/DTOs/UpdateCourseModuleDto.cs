using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class UpdateCourseModuleDto
{
    [Required(ErrorMessage = "عنوان سرفصل الزامی است")]
    [MinLength(2, ErrorMessage = "عنوان سرفصل باید حداقل ۲ کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
