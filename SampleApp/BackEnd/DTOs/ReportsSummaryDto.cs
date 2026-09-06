namespace BackEnd.DTOs;

public class ReportsSummaryDto
{
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalEnrollments { get; set; }

    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalCancelled { get; set; }
}