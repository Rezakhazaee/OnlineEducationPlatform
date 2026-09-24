using System.Net.Http.Json;
using System.Text.Json;
using BackEnd.Data;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class ZarinPalService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _db;

    public ZarinPalService(
        IHttpClientFactory httpClientFactory,
        ApplicationDbContext db)
    {
        _httpClientFactory = httpClientFactory;
        _db = db;
    }

    public async Task<(bool Success, string? Authority, string? PaymentUrl, string? Error)>
        RequestPaymentAsync(
            decimal amount,
            string callbackUrl,
            string description,
            string? mobile)
    {
        var settings = await GetSettingsAsync();

        if (settings == null)
        {
            return (
                false,
                null,
                null,
                "تنظیمات درگاه پرداخت در سیستم ثبت نشده است.");
        }

        if (!settings.IsActive)
        {
            return (
                false,
                null,
                null,
                "درگاه پرداخت در حال حاضر غیرفعال است.");
        }

        if (string.IsNullOrWhiteSpace(settings.MerchantId))
        {
            return (
                false,
                null,
                null,
                "MerchantId زرین‌پال در تنظیمات درگاه وارد نشده است.");
        }

        var apiBaseUrl = GetApiBaseUrl(settings);

        var paymentBaseUrl = GetPaymentBaseUrl(settings);

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            return (
                false,
                null,
                null,
                "API URL درگاه پرداخت تنظیم نشده است.");
        }

        if (string.IsNullOrWhiteSpace(paymentBaseUrl))
        {
            return (
                false,
                null,
                null,
                "Payment URL درگاه پرداخت تنظیم نشده است.");
        }

        var amountValue =
            Convert.ToInt64(
                Math.Round(
                    amount * 10m,
                    0,
                    MidpointRounding.AwayFromZero));

        var body = new
        {
            merchant_id = settings.MerchantId,
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
                var errorMessage =
                    "پاسخ نامعتبر از زرین‌پال دریافت شد.";

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
        var settings = await GetSettingsAsync();

        if (settings == null)
        {
            return (
                false,
                null,
                0,
                "تنظیمات درگاه پرداخت در سیستم ثبت نشده است.");
        }

        if (!settings.IsActive)
        {
            return (
                false,
                null,
                0,
                "درگاه پرداخت در حال حاضر غیرفعال است.");
        }

        if (string.IsNullOrWhiteSpace(settings.MerchantId))
        {
            return (
                false,
                null,
                0,
                "MerchantId زرین‌پال در تنظیمات درگاه وارد نشده است.");
        }

        var apiBaseUrl = GetApiBaseUrl(settings);

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            return (
                false,
                null,
                0,
                "API URL درگاه پرداخت تنظیم نشده است.");
        }

        var amountValue =
            Convert.ToInt64(
                Math.Round(
                    amount * 10m,
                    0,
                    MidpointRounding.AwayFromZero));

        var body = new
        {
            merchant_id = settings.MerchantId,
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

            if (!json.RootElement.TryGetProperty(
                    "data",
                    out var data) ||
                !data.TryGetProperty(
                    "code",
                    out _))
            {
                var errorCode = 0;
                string? errorMessage = null;

                if (json.RootElement.TryGetProperty(
                        "errors",
                        out var errors))
                {
                    if (errors.TryGetProperty(
                            "code",
                            out var errorCodeProperty) &&
                        errorCodeProperty.TryGetInt32(
                            out var parsedErrorCode))
                    {
                        errorCode = parsedErrorCode;
                    }

                    if (errors.TryGetProperty(
                            "message",
                            out var errorMessageProperty))
                    {
                        errorMessage =
                            errorMessageProperty.GetString();
                    }
                }

                return (
                    false,
                    null,
                    errorCode,
                    errorMessage
                    ?? "پاسخ نامعتبر از زرین‌پال دریافت شد.");
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

    private async Task<BackEnd.Models.PaymentGatewaySettings?> GetSettingsAsync()
    {
        return await _db.PaymentGatewaySettings
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }

    private static string? GetApiBaseUrl(
        BackEnd.Models.PaymentGatewaySettings settings)
    {
        if (string.Equals(
                settings.Mode,
                "Production",
                StringComparison.OrdinalIgnoreCase))
        {
            return settings.ApiBaseUrl;
        }

        return settings.ApiBaseUrl;
    }

    private static string? GetPaymentBaseUrl(
        BackEnd.Models.PaymentGatewaySettings settings)
    {
        if (string.Equals(
                settings.Mode,
                "Production",
                StringComparison.OrdinalIgnoreCase))
        {
            return settings.PaymentBaseUrl;
        }

        return settings.PaymentBaseUrl;
    }
}
