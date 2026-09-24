namespace BackEnd.DTOs;

public class PaymentReportResultDto
{
    public List<PaymentReportItemDto> Items { get; set; } = new();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }

    public decimal TotalAmount { get; set; }
}
