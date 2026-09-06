using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FrontEnd.Services;

public class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public string? Token { get; private set; }
    public int? UserId { get; private set; }
    public string? FullName { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }

    public bool IsLoggedIn =>
        !string.IsNullOrWhiteSpace(Token);

    public bool IsAdmin =>
        Role == "Admin";

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
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

        return (
            true,
            result.Message ?? "ورود موفق بود"
        );
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