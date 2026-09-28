using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace FrontEnd.Services;

public class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJSRuntime _js;

    private const string TokenKey = "oe_token";
    private const string UserIdKey = "oe_user_id";
    private const string FullNameKey = "oe_full_name";
    private const string UsernameKey = "oe_username";
    private const string RoleKey = "oe_role";

    public string? Token { get; private set; }
    public int? UserId { get; private set; }
    public string? FullName { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }

    public bool IsLoggedIn =>
        !string.IsNullOrWhiteSpace(Token);

    public bool IsAdmin =>
        Role == "Admin";

    public bool IsStudent =>
        Role == "Student";

    public AuthService(
        IHttpClientFactory httpClientFactory,
        IJSRuntime js)
    {
        _httpClientFactory = httpClientFactory;
        _js = js;
    }

    public async Task RestoreAsync()
    {
        try
        {
            Token = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                TokenKey);

            var userIdValue = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                UserIdKey);

            FullName = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                FullNameKey);

            Username = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                UsernameKey);

            Role = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                RoleKey);

            if (int.TryParse(userIdValue, out var userId))
            {
                UserId = userId;
            }
            else
            {
                UserId = null;
            }
        }
        catch
        {
            Logout();
        }
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
            request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content
                .ReadFromJsonAsync<LoginErrorResponse>();

            return (
                false,
                error?.Message
                    ?? "نام کاربری یا رمز عبور اشتباه است");
        }

        var result = await response.Content
            .ReadFromJsonAsync<LoginResponse>();

        if (result == null ||
            string.IsNullOrWhiteSpace(result.Token))
        {
            return (
                false,
                "پاسخ ورود از سرور معتبر نیست");
        }

        Token = result.Token;
        UserId = result.User?.Id;
        FullName = result.User?.FullName;
        Username = result.User?.Username;
        Role = result.User?.Role;

        await SaveStateAsync();

        return (
            true,
            result.Message ?? "ورود موفق بود");
    }

    private async Task SaveStateAsync()
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

    private async Task ClearStoredStateAsync()
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

    public HttpClient CreateAuthenticatedClient()
    {
        var client = _httpClientFactory
            .CreateClient("Backend");

        if (!string.IsNullOrWhiteSpace(Token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    Token);
        }

        return client;
    }

    public async Task LogoutAsync()
    {
        Logout();

        try
        {
            await ClearStoredStateAsync();
        }
        catch
        {
        }
    }

    public void Logout()
    {
        Token = null;
        UserId = null;
        FullName = null;
        Username = null;
        Role = null;
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
