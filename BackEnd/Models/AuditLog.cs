namespace BackEnd.Models;
public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Entity { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}