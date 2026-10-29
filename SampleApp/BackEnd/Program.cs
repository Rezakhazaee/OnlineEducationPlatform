using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using BackEnd.Data;
using BackEnd.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.UseSystemFonts = false;
QuestPDF.Settings.FontDiscoveryPath = "Fonts";

// Controllers
builder.Services.AddControllers();

// Database - SQLite: resolve relative paths against Backend.
var cs = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is missing");
var sqlite = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(cs);
if (!Path.IsPathRooted(sqlite.DataSource))
    sqlite.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqlite.DataSource);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(sqlite.ToString()));

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

var app = builder.Build();

 // =========================
 // Seed Database
 // =========================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();


    // FIN-06-C: prevent duplicate paid installments in the existing SQLite database.
    db.Database.ExecuteSqlRaw("""
        CREATE UNIQUE INDEX IF NOT EXISTS "UX_Payments_PaidInstallment"
        ON "Payments" ("EnrollmentId", "PaymentType")
        WHERE "Status" = 'Paid' AND "PaymentType" <> 'FullPayment';
        """);

    // DB-ORG-01-A: allow at most one organization settings row.
    db.Database.ExecuteSqlRaw("""
        CREATE TRIGGER IF NOT EXISTS "TR_OrganizationSettings_OnlyOne"
        BEFORE INSERT ON "OrganizationSettings"
        WHEN EXISTS (SELECT 1 FROM "OrganizationSettings")
        BEGIN
            SELECT RAISE(ABORT, 'Only one OrganizationSettings row is allowed.');
        END;
        """);
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