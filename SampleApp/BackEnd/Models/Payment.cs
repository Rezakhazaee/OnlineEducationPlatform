namespace BackEnd.Models;

public class Payment
{
    public int Id { get; set; }


    // مربوط به کدام ثبت نام است؟
    public int EnrollmentId { get; set; }

    public Enrollment? Enrollment { get; set; }


    // مبلغ پرداختی
    public decimal Amount { get; set; }


    // تاریخ پرداخت
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    // تاریخ سررسید قسط
    // برای پرداخت کامل می‌تواند خالی باشد
    public DateTime? DueDate { get; set; }



    // نوع پرداخت
    // مثال: پیش پرداخت، قسط اول، قسط دوم
    public string PaymentType { get; set; } = string.Empty;


    // توضیحات اضافی
    public string? Description { get; set; }


    // وضعیت پرداخت
    public string Status { get; set; } = "Paid";

    public string? PaymentMethod { get; set; }

    public string? GatewayAuthority { get; set; }

    public string? GatewayRefId { get; set; }
}