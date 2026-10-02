namespace BackEnd.Models;

public class Certificate
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }

    public string CertificateNumber { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public DateTime IssuedAtUtc { get; set; }
}
