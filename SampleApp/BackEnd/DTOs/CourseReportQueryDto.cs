namespace BackEnd.DTOs;

public class CourseReportQueryDto
{
    public string? Search { get; set; }
    public int? InstructorId { get; set; }
    public string? DeliveryType { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public string SortBy { get; set; } = "title";
    public bool SortDescending { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
