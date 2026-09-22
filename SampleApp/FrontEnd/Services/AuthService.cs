using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace FrontEnd.Services;

public class AuthService
{
    private const string TokenKey = "auth.token";
    private const string UserIdKey = "auth.userId";
    private const string FullNameKey = "auth.fullName";
    private const string UsernameKey = "auth.username";
    private const string RoleKey = "auth.role";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJSRuntime _js;

    public string? Token { get; private set; }
    public int? UserId { get; private set; }
    public string? FullName { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }

    public bool IsLoggedIn =>
        !string.IsNullOrWhiteSpace(Token);

    public bool IsAdmin =>
        Role == "Admin";

    public bool IsEducationStaff =>
        Role == "EducationStaff";

    public bool IsMarketer =>
        Role == "Marketer";

    public bool IsSupport =>
        Role == "Support";

    public bool IsInstructor =>
        Role == "Instructor";

    public bool IsStudent =>
        Role == "Student";

    public bool IsManagementPanelUser =>
        IsAdmin || IsEducationStaff;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        IJSRuntime js)
    {
        _httpClientFactory = httpClientFactory;
        _js = js;
    }

    public async Task<(bool Success, string Message)> Login(
        string username,
        string password)
    {
        var client = _httpClientFactory
            .CreateClient("Backend");

        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var response = await client.PostAsJsonAsync(
            "api/Auth/login",
            request
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content
                .ReadFromJsonAsync<LoginErrorResponse>();

            return (
                false,
                error?.Message
                    ?? "نام کاربری یا رمز عبور اشتباه است"
            );
        }

        var result = await response.Content
            .ReadFromJsonAsync<LoginResponse>();

        if (result == null ||
            string.IsNullOrWhiteSpace(result.Token))
        {
            return (
                false,
                "پاسخ ورود از سرور معتبر نیست"
            );
        }

        Token = result.Token;
        UserId = result.User?.Id;
        FullName = result.User?.FullName;
        Username = result.User?.Username;
        Role = result.User?.Role;

        await SaveToStorage();

        return (
            true,
            result.Message ?? "ورود موفق بود"
        );
    }

    public async Task RestoreAsync()
    {
        try
        {
            Token = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                TokenKey);

            if (string.IsNullOrWhiteSpace(Token))
            {
                ClearMemory();
                return;
            }

            var userIdValue = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                UserIdKey);

            UserId =
                int.TryParse(userIdValue, out var userId)
                    ? userId
                    : null;

            FullName = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                FullNameKey);

            Username = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                UsernameKey);

            Role = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                RoleKey);
        }
        catch
        {
            ClearMemory();
        }
    }

    private async Task SaveToStorage()
    {
        await _js.InvokeVoidAsync(
            "localStorage.setItem",
            TokenKey,
            Token ?? string.Empty);

        await _js.InvokeVoidAsync(
            "localStorage.setItem",
            UserIdKey,
            UserId?.ToString() ?? string.Empty);

        await _js.InvokeVoidAsync(
            "localStorage.setItem",
            FullNameKey,
            FullName ?? string.Empty);

        await _js.InvokeVoidAsync(
            "localStorage.setItem",
            UsernameKey,
            Username ?? string.Empty);

        await _js.InvokeVoidAsync(
            "localStorage.setItem",
            RoleKey,
            Role ?? string.Empty);
    }

    public async Task LogoutAsync()
    {
        ClearMemory();

        try
        {
            await _js.InvokeVoidAsync(
                "localStorage.removeItem",
                TokenKey);

            await _js.InvokeVoidAsync(
                "localStorage.removeItem",
                UserIdKey);

            await _js.InvokeVoidAsync(
                "localStorage.removeItem",
                FullNameKey);

            await _js.InvokeVoidAsync(
                "localStorage.removeItem",
                UsernameKey);

            await _js.InvokeVoidAsync(
                "localStorage.removeItem",
                RoleKey);
        }
        catch
        {
            // در صورت نبود دسترسی به localStorage،
            // وضعیت حافظه‌ای قبلاً پاک شده است.
        }
    }

    private void ClearMemory()
    {
        Token = null;
        UserId = null;
        FullName = null;
        Username = null;
        Role = null;
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = _httpClientFactory
            .CreateClient("Backend");

        if (!string.IsNullOrWhiteSpace(Token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    Token
                );
        }

        return client;
    }
}


/* =========================
   Login Request
========================= */

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}


/* =========================
   Login Response
========================= */

public class LoginResponse
{
    public string? Message { get; set; }

    public string? Token { get; set; }

    public LoginUser? User { get; set; }
}


public class LoginUser
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? Username { get; set; }

    public string? Role { get; set; }
}


/* =========================
   Error Response
========================= */

public class LoginErrorResponse
{
    public string? Message { get; set; }
}
