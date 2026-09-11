using System.Net.Http.Json;

namespace FrontEnd.Services;

public class ThemeService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public string PrimaryColor { get; private set; } = "#568fa8";

    public string SecondaryColor { get; private set; } = "#263d4a";

    public bool IsActive { get; private set; } = true;

    public ThemeService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task LoadThemeAsync()
    {
        try
        {
            var client = _httpClientFactory
                .CreateClient("Backend");

            var settings = await client.GetFromJsonAsync<OrganizationSettingsResponse>(
                "api/OrganizationSettings"
            );

            if (settings == null)
                return;

            if (!string.IsNullOrWhiteSpace(settings.PrimaryColor))
                PrimaryColor = settings.PrimaryColor;

            if (!string.IsNullOrWhiteSpace(settings.SecondaryColor))
                SecondaryColor = settings.SecondaryColor;

            IsActive = settings.ThemeIsActive;
        }
        catch
        {
            // در صورت در دسترس نبودن Backend،
            // رنگ‌های پیش‌فرض استفاده می‌شوند.
        }
    }

    private class OrganizationSettingsResponse
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public string? PrimaryColor { get; set; }

        public string? SecondaryColor { get; set; }

        public bool ThemeIsActive { get; set; }
    }
}