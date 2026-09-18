using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class CreateCourseLessonDto
{
    public int CourseModuleId { get; set; }

    [Required(ErrorMessage = "عنوان درس الزامی است")]
    [MinLength(2, ErrorMessage = "عنوان درس باید حداقل ۲ کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [RegularExpression(
        "^(Video|Text)$",
        ErrorMessage = "نوع محتوا باید Video یا Text باشد")]
    public string ContentType { get; set; } = "Video";

    public string? ContentUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int DurationMinutes { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFreePreview { get; set; } = false;
}
