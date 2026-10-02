namespace BackEnd.Models;

public class Installment
{
    public int Id { get; set; }

    // مربوط به کدام ثبت‌نام است؟
    public int EnrollmentId { get; set; }

    public Enrollment? Enrollment { get; set; }

    // شماره قسط: 1، 2 یا 3
    public int InstallmentNumber { get; set; }

    // مبلغ قسط
    public decimal Amount { get; set; }

    // تاریخ سررسید
    public DateTime DueDate { get; set; }

    // وضعیت برنامه‌ای قسط
    // Pending / Paid / Cancelled
    public string Status { get; set; } = "Pending";

    // توضیحات
    public string? Description { get; set; }

    // پرداخت‌های مربوط به این قسط
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
