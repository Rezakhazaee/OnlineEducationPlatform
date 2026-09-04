namespace BackEnd.Models;

public class StudentFollowUp
{
    public int Id { get; set; }

    // دانشجو
    public int StudentId { get; set; }

    public Student? Student { get; set; }

    // پشتیبانی که پیگیری را ثبت کرده
    public int SupportUserId { get; set; }

    public User? SupportUser { get; set; }

    // تاریخ پیگیری
    public DateTime FollowUpDate { get; set; } = DateTime.Now;

    // وضعیت پیگیری
    public string Status { get; set; } = "Pending";

    // نتیجه / توضیحات پیگیری
    public string? Description { get; set; }

    // تاریخ ایجاد رکورد
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}