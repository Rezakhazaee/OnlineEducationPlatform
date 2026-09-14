using System.Net.Http.Json;

namespace FrontEnd.Services;

public class ThemeService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public string PrimaryColor { get; private set; } = "#568fa8";
    public string SecondaryColor { get; private set; } = "#263d4a";

    public string PrimaryLight { get; private set; } = "#70abc0";
    public string PrimaryDark { get; private set; } = "#477f99";

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

            var primary = NormalizeColor(settings.PrimaryColor, "#568fa8");
            var secondary = NormalizeColor(settings.SecondaryColor, "#263d4a");

            IsActive = settings.ThemeIsActive;

            if (!IsActive)
            {
                PrimaryColor = "#568fa8";
                SecondaryColor = "#263d4a";
                PrimaryLight = "#70abc0";
                PrimaryDark = "#477f99";
                return;
            }

            PrimaryColor = primary;
            SecondaryColor = secondary;

            PrimaryLight = LightenColor(primary, 18);
            PrimaryDark = DarkenColor(primary, 12);
        }
        catch
        {
            // در صورت در دسترس نبودن Backend،
            // رنگ‌های پیش‌فرض استفاده می‌شوند.
        }
    }

    private static string NormalizeColor(string? color, string fallback)
    {
        if (string.IsNullOrWhiteSpace(color))
            return fallback;

        color = color.Trim();

        if (color.Length != 7 || color[0] != '#')
            return fallback;

        if (!color.Skip(1).All(Uri.IsHexDigit))
            return fallback;

        return color;
    }

    private static string LightenColor(string hex, int amount)
    {
        if (!TryParseHex(hex, out var r, out var g, out var b))
            return "#70abc0";

        r = r + (255 - r) * amount / 100;
        g = g + (255 - g) * amount / 100;
        b = b + (255 - b) * amount / 100;

        return ToHex(r, g, b);
    }

    private static string DarkenColor(string hex, int amount)
    {
        if (!TryParseHex(hex, out var r, out var g, out var b))
            return "#477f99";

        r = r * (100 - amount) / 100;
        g = g * (100 - amount) / 100;
        b = b * (100 - amount) / 100;

        return ToHex(r, g, b);
    }

    private static bool TryParseHex(
        string hex,
        out int r,
        out int g,
        out int b)
    {
        r = 0;
        g = 0;
        b = 0;

        if (hex.Length != 7 || hex[0] != '#')
            return false;

        if (!int.TryParse(
                hex.Substring(1, 2),
                System.Globalization.NumberStyles.HexNumber,
                null,
                out r))
            return false;

        if (!int.TryParse(
                hex.Substring(3, 2),
                System.Globalization.NumberStyles.HexNumber,
                null,
                out g))
            return false;

        if (!int.TryParse(
                hex.Substring(5, 2),
                System.Globalization.NumberStyles.HexNumber,
                null,
                out b))
            return false;

        return true;
    }

    private static string ToHex(int r, int g, int b)
    {
        return $"#{r:X2}{g:X2}{b:X2}";
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