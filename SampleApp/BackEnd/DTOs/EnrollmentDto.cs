namespace BackEnd.DTOs;

public class EnrollmentDto
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string? StudentName { get; set; }

    public int CourseId { get; set; }

    public string? CourseTitle { get; set; }

    // قیمت نهایی ثبت‌نام
    public decimal CoursePrice { get; set; }

    // مجموع پرداخت‌های تاییدشده
    public decimal TotalPaid { get; set; }

    // مبلغ باقی‌مانده
    public decimal RemainingAmount { get; set; }

    // نوع برگزاری دوره
    public string DeliveryType { get; set; } = string.Empty;

    // هشدار مالی برای آموزشگاه (پرداخت ناقص دوره حضوری)
    public bool HasPaymentWarning { get; set; }

    // وضعیت مالی
    public string PaymentStatus { get; set; } = string.Empty;

    public int? SupportUserId { get; set; }

    public string? SupportUserName { get; set; }

    public int? InstructorId { get; set; }

    public string? InstructorName { get; set; }

    public DateTime StartDate { get; set; }

    public string Status { get; set; } = string.Empty;

    // توضیحات ثبت‌نام
    public string? Description { get; set; }

    // سازمان طرف قرارداد این ثبت‌نام
    public CoursePartnerOrganizationDto? CoursePartnerOrganization { get; set; }
}