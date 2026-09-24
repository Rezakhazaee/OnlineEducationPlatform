namespace BackEnd.DTOs;

public class CourseReportItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string DeliveryType { get; set; } = string.Empty;

    public int? InstructorId { get; set; }
    public string? InstructorName { get; set; }

    public bool IsActive { get; set; }
}
