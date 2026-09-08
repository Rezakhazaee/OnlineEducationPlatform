using BackEnd.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Database - SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key is not configured"
    );

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Jwt:Issuer is not configured"
    );

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Jwt:Audience is not configured"
    );

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
    });

// Authorization
builder.Services.AddAuthorization();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();
// =========================
// Seed Main Admin
// =========================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var adminExists = await db.Users.AnyAsync(u => u.Id == 1);

    if (!adminExists)
    {
        var admin = new BackEnd.Models.User
        {
            FullName = "System Administrator",
            Mobile = "09000000000",
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}

// OpenAPI + Scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// HTTPS
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();
