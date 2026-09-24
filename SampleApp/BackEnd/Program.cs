using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using BackEnd.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using BackEnd.Services;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.UseSystemFonts = true;

// Controllers
builder.Services.AddControllers();

// Database - SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Package access
builder.Services.AddScoped<PackageAccessService>();

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

builder.Services.AddHttpClient();
builder.Services.AddScoped<ZarinPalService>();
var app = builder.Build();

 // =========================
 // Seed Database
 // =========================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await db.Database.MigrateAsync();

    var packageLevel = builder.Configuration.GetValue<int?>("Product:PackageLevel") ?? 1;
    var organizationName = builder.Configuration["Product:OrganizationName"] ?? "Sample Organization";

    var initialAdminUsername =
        builder.Configuration["Product:InitialAdminUsername"];

    var initialAdminPassword =
        builder.Configuration["Product:InitialAdminPassword"];

    if (app.Environment.IsDevelopment())
    {
        initialAdminUsername ??= "admin";
        initialAdminPassword ??= "Admin@12345";
    }
    else if (string.IsNullOrWhiteSpace(initialAdminUsername) ||
             string.IsNullOrWhiteSpace(initialAdminPassword))
    {
        throw new InvalidOperationException(
            "Production requires Product:InitialAdminUsername and Product:InitialAdminPassword.");
    }

    await DbSeeder.SeedAsync(
        db,
        packageLevel,
        app.Environment.IsDevelopment(),
        initialAdminUsername!,
        initialAdminPassword!,
        organizationName);
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