using System.Net.Http.Json;
using System.Text.Json;

namespace BackEnd.Services;

public class ZarinPalService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ZarinPalService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<(bool Success, string? Authority, string? PaymentUrl, string? Error)>
        RequestPaymentAsync(
            decimal amount,
            string callbackUrl,
            string description,
            string? mobile)
    {
        var merchantId =
            _configuration["ZarinPal:MerchantId"];

        if (string.IsNullOrWhiteSpace(merchantId))
        {
            return (
                false,
                null,
                null,
                "MerchantId زرین‌پال در تنظیمات سیستم وارد نشده است.");
        }

        var apiBaseUrl =
            _configuration["ZarinPal:ApiBaseUrl"]
            ?? "https://sandbox.zarinpal.com/pg/v4/payment/";

        var paymentBaseUrl =
            _configuration["ZarinPal:PaymentBaseUrl"]
            ?? "https://sandbox.zarinpal.com/pg/StartPay/";

          var amountValue =
              Convert.ToInt64(
                  Math.Round(
                      amount * 10m,
                      0,
                      MidpointRounding.AwayFromZero));

        var body = new
        {
            merchant_id = merchantId,
            amount = amountValue,
            callback_url = callbackUrl,
            description = description,
            metadata = new
            {
                mobile
            }
        };

        try
        {
            var client =
                _httpClientFactory.CreateClient();

            using var response =
                await client.PostAsJsonAsync(
                    apiBaseUrl.TrimEnd('/') + "/request.json",
                    body);

            var content =
                await response.Content.ReadAsStringAsync();

            using var json =
                JsonDocument.Parse(content);

            if (!json.RootElement.TryGetProperty(
                    "data",
                    out var data))
            {
                var errorMessage = "پاسخ نامعتبر از زرین‌پال دریافت شد.";

                if (json.RootElement.TryGetProperty(
                        "errors",
                        out var errors) &&
                    errors.TryGetProperty(
                        "message",
                        out var errorText))
                {
                    errorMessage =
                        errorText.GetString()
                        ?? errorMessage;
                }

                return (
                    false,
                    null,
                    null,
                    errorMessage);
            }

            var code =
                data.TryGetProperty(
                    "code",
                    out var codeProperty)
                    ? codeProperty.GetInt32()
                    : 0;

            if (code != 100 ||
                !data.TryGetProperty(
                    "authority",
                    out var authorityProperty))
            {
                return (
                    false,
                    null,
                    null,
                    $"درخواست پرداخت توسط زرین‌پال پذیرفته نشد. کد: {code}");
            }

            var authority =
                authorityProperty.GetString();

            if (string.IsNullOrWhiteSpace(authority))
            {
                return (
                    false,
                    null,
                    null,
                    "Authority از زرین‌پال دریافت نشد.");
            }

            var paymentUrl =
                paymentBaseUrl.TrimEnd('/') +
                "/" +
                authority;

            return (
                true,
                authority,
                paymentUrl,
                null);
        }
        catch (Exception ex)
        {
            return (
                false,
                null,
                null,
                "ارتباط با زرین‌پال برقرار نشد: " +
                ex.Message);
        }
    }

    public async Task<(bool Success, string? RefId, int Code, string? Error)>
        VerifyPaymentAsync(
            decimal amount,
            string authority)
    {
        var merchantId =
            _configuration["ZarinPal:MerchantId"];

        if (string.IsNullOrWhiteSpace(merchantId))
        {
            return (
                false,
                null,
                0,
                "MerchantId زرین‌پال در تنظیمات سیستم وارد نشده است.");
        }

        var apiBaseUrl =
            _configuration["ZarinPal:ApiBaseUrl"]
            ?? "https://sandbox.zarinpal.com/pg/v4/payment/";

          var amountValue =
              Convert.ToInt64(
                  Math.Round(
                      amount * 10m,
                      0,
                      MidpointRounding.AwayFromZero));

        var body = new
        {
            merchant_id = merchantId,
            amount = amountValue,
            authority
        };

        try
        {
            var client =
                _httpClientFactory.CreateClient();

            using var response =
                await client.PostAsJsonAsync(
                    apiBaseUrl.TrimEnd('/') + "/verify.json",
                    body);

            var content =
                await response.Content.ReadAsStringAsync();

            using var json =
                JsonDocument.Parse(content);

              if (!json.RootElement.TryGetProperty("data", out var data) ||
                  !data.TryGetProperty("code", out _))
              {
                  var errorCode = 0;
                  string? errorMessage = null;

                  if (json.RootElement.TryGetProperty("errors", out var errors))
                  {
                      if (errors.TryGetProperty("code", out var errorCodeProperty) &&
                          errorCodeProperty.TryGetInt32(out var parsedErrorCode))
                      {
                          errorCode = parsedErrorCode;
                      }

                      if (errors.TryGetProperty("message", out var errorMessageProperty))
                      {
                          errorMessage = errorMessageProperty.GetString();
                      }
                  }

                  return (
                      false,
                      null,
                      errorCode,
                      errorMessage ?? "پاسخ نامعتبر از زرین‌پال دریافت شد.");
              }
            var code =
                data.TryGetProperty(
                    "code",
                    out var codeProperty)
                    ? codeProperty.GetInt32()
                    : 0;

            string? refId = null;

            if (data.TryGetProperty(
                    "ref_id",
                    out var refIdProperty))
            {
                refId =
                    refIdProperty.ToString();
            }

            return (
                code == 100 || code == 101,
                refId,
                code,
                null);
        }
        catch (Exception ex)
        {
            return (
                false,
                null,
                0,
                "ارتباط با زرین‌پال برقرار نشد: " +
                ex.Message);
        }
    }
}
