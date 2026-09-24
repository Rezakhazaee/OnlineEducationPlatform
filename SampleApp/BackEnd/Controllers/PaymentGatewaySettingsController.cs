using BackEnd.Data;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PaymentGatewaySettingsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public PaymentGatewaySettingsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _db.PaymentGatewaySettings
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new PaymentGatewaySettings();
        }

        return Ok(new
        {
            settings.Id,
            settings.GatewayName,
            settings.IsActive,
            settings.Mode,
            settings.MerchantId,
            settings.ApiBaseUrl,
            settings.PaymentBaseUrl,
            settings.CallbackBaseUrl,
            settings.FrontendBaseUrl,
            settings.UpdatedAt
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] PaymentGatewaySettings model)
    {
        var settings = await _db.PaymentGatewaySettings
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new PaymentGatewaySettings();
            _db.PaymentGatewaySettings.Add(settings);
        }

        settings.GatewayName =
            string.IsNullOrWhiteSpace(model.GatewayName)
                ? "ZarinPal"
                : model.GatewayName.Trim();

        settings.IsActive = model.IsActive;

        settings.Mode =
            string.Equals(
                model.Mode,
                "Production",
                StringComparison.OrdinalIgnoreCase)
                ? "Production"
                : "Sandbox";

        settings.MerchantId =
            string.IsNullOrWhiteSpace(model.MerchantId)
                ? null
                : model.MerchantId.Trim();

        settings.ApiBaseUrl =
            string.IsNullOrWhiteSpace(model.ApiBaseUrl)
                ? null
                : model.ApiBaseUrl.Trim();

        settings.PaymentBaseUrl =
            string.IsNullOrWhiteSpace(model.PaymentBaseUrl)
                ? null
                : model.PaymentBaseUrl.Trim();

        settings.CallbackBaseUrl =
            string.IsNullOrWhiteSpace(model.CallbackBaseUrl)
                ? null
                : model.CallbackBaseUrl.Trim();

        settings.FrontendBaseUrl =
            string.IsNullOrWhiteSpace(model.FrontendBaseUrl)
                ? null
                : model.FrontendBaseUrl.Trim();

        settings.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "تنظیمات درگاه با موفقیت ذخیره شد.",
            settings.Id,
            settings.GatewayName,
            settings.IsActive,
            settings.Mode,
            settings.MerchantId,
            settings.ApiBaseUrl,
            settings.PaymentBaseUrl,
            settings.CallbackBaseUrl,
            settings.FrontendBaseUrl,
            settings.UpdatedAt
        });
    }
}
