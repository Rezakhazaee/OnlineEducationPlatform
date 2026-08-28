namespace BackEnd.DTOs;

public class StudentDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string NationalCode { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public string Mobile { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? GuardianName { get; set; }

    public string? GuardianMobile { get; set; }

    public int? OrganizationId { get; set; }

    public int? MarketingUserId { get; set; }

    public int? SupportUserId { get; set; }

    public DateTime CreatedDate { get; set; }
}