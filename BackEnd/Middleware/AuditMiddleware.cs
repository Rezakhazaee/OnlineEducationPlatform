using System.Security.Claims;
using BackEnd.Data;
using BackEnd.Models;
namespace BackEnd.Middleware;
public sealed class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditMiddleware> _logger;
    public AuditMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    private static bool IsSensitiveReadPath(string path)
    {
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !string.Equals(parts[0], "api", StringComparison.OrdinalIgnoreCase))
            return false;

        var resource = parts[1];
        if (resource.Equals("Enrollments", StringComparison.OrdinalIgnoreCase) ||
            resource.Equals("Payments", StringComparison.OrdinalIgnoreCase) ||
            resource.Equals("Users", StringComparison.OrdinalIgnoreCase) ||
            resource.Equals("StudentFollowUps", StringComparison.OrdinalIgnoreCase) ||
            resource.Equals("AuditLogs", StringComparison.OrdinalIgnoreCase))
            return true;

        if (resource.Equals("Students", StringComparison.OrdinalIgnoreCase))
            return parts.Length == 2 ||
                   (parts.Length >= 3 &&
                    (parts[2].Equals("me", StringComparison.OrdinalIgnoreCase) ||
                     int.TryParse(parts[2], out _)));

        return resource.Equals("Certificates", StringComparison.OrdinalIgnoreCase) &&
               parts.Length >= 3 &&
               parts[2].Equals("enrollment", StringComparison.OrdinalIgnoreCase);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";
        var isWrite = method == HttpMethods.Post || method == HttpMethods.Put ||
                      method == HttpMethods.Patch || method == HttpMethods.Delete;
        var isSensitiveRead = method == HttpMethods.Get && IsSensitiveReadPath(path);
        if (!isWrite && !isSensitiveRead)
        {
            await _next(context);
            return;
        }
        try { await _next(context); }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            try
            {
                var idText = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = int.TryParse(idText, out var id) ? id : null;
                var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.AuditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    Username = context.User.Identity?.Name,
                    Role = context.User.FindFirst(ClaimTypes.Role)?.Value,
                    Action = method + " " + path,
                    Entity = parts.Length > 1 ? parts[1] : null,
                    HttpMethod = method,
                    Path = path,
                    StatusCode = context.Response.StatusCode,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist audit event for {Method} {Path}", method, context.Request.Path);
            }
        }
    }
}
