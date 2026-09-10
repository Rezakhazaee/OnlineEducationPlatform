using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace FrontEnd.Services;

public class ThemeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public string PrimaryColor { get; private set; } = "#568fa8";
    public string SecondaryColor { get; private set; } = "#263d4a";
    public bool IsActive { get; private set; } = true;

    public ThemeService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task LoadThemeAsync()
    {
        var organizationId =
            _configuration.GetValue<int>("OrganizationId");

        if (organizationId <= 0)
            return;

        try
        {
            var client = _httpClientFactory
                .CreateClient("Backend");

            var theme = await client.GetFromJsonAsync<ThemeResponse>(
                $"api/OrganizationThemes/{organizationId}"
            );

            if (theme == null)
                return;

            if (!string.IsNullOrWhiteSpace(theme.PrimaryColor))
                PrimaryColor = theme.PrimaryColor;

            if (!string.IsNullOrWhiteSpace(theme.SecondaryColor))
                SecondaryColor = theme.SecondaryColor;

            IsActive = theme.IsActive;
        }
        catch
        {
            // در صورت در دسترس نبودن Backend،
            // رنگ‌های پیش‌فرض استفاده می‌شوند.
        }
    }

    private class ThemeResponse
    {
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public bool IsActive { get; set; }
    }
}
