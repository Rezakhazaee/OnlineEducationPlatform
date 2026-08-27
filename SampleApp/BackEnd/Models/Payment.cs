namespace BackEnd.Models;

public class Payment
{
    public int Id { get; set; }


    // مربوط به کدام ثبت نام است؟
    public int EnrollmentId { get; set; }


    // مبلغ پرداختی
    public decimal Amount { get; set; }


    // تاریخ پرداخت
    public DateTime PaymentDate { get; set; } = DateTime.Now;


    // نوع پرداخت
    // مثال: پیش پرداخت، قسط اول، قسط دوم
    public string PaymentType { get; set; } = string.Empty;


    // توضیحات اضافی
    public string? Description { get; set; }


    // وضعیت پرداختgit status
    public string Status { get; set; } = "Paid";
}