namespace BackEnd.Models;

public class PaymentGatewaySettings
{
    public int Id { get; set; }

    public string GatewayName { get; set; } = "ZarinPal";

    public bool IsActive { get; set; } = false;

    public string Mode { get; set; } = "Sandbox";

    public string? MerchantId { get; set; }

    public string? ApiBaseUrl { get; set; }

    public string? PaymentBaseUrl { get; set; }

    public string? CallbackBaseUrl { get; set; }

    public string? FrontendBaseUrl { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
