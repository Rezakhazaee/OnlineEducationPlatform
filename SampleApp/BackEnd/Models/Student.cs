namespace BackEnd.Models;

public class Student
{
    public int Id { get; set; }

    // ارتباط با حساب کاربری
    public int? UserId { get; set; }

    public User? User { get; set; }


    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string NationalCode { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public string Mobile { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? GuardianName { get; set; }

    public string? GuardianMobile { get; set; }


    // سازمان
    public int? OrganizationId { get; set; }

    public Organization? Organization { get; set; }


    // کاربر بازاریابی
    public int? MarketingUserId { get; set; }

    public User? MarketingUser { get; set; }


    // کاربری که دانشجو را ایجاد کرده
    public int? CreatedByUserId { get; set; }

    public User? CreatedByUser { get; set; }


    // پشتیبان آموزشی
    public int? SupportUserId { get; set; }

    public User? SupportUser { get; set; }


    public DateTime CreatedDate { get; set; } = DateTime.Now;
}