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

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdValue = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdValue, out var userId))
                {
                    context.Fail("Invalid user identity.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;

                if (db == null)
                {
                    context.Fail("User database is unavailable.");
                    return;
                }

                var isActive = await db.Users.AsNoTracking().AnyAsync(u =>
                    u.Id == userId && u.IsActive &&
                    (u.Role != nameof(BackEnd.Models.PartnerOrganization) ||
                     (u.PartnerOrganizationId.HasValue &&
                      db.PartnerOrganizations.Any(org =>
                          org.Id == u.PartnerOrganizationId.Value && org.IsActive))));

                if (!isActive)
                    context.Fail("User is inactive or does not exist.");
            }
        };
    });

// Authorization
builder.Services.AddAuthorization();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
    if (feature?.Error is Exception exception)
        context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler")
            .LogError(exception, "Unhandled backend exception for {Path}", context.Request.Path);

    if (context.Response.HasStarted) return;
    context.Response.Clear();
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/json; charset=utf-8";
    await context.Response.WriteAsJsonAsync(new { message = "Internal server error." });
}));


// SEC-08: create the audit table in the existing SQLite database only.
using (var auditScope = app.Services.CreateScope())
{
    var auditDb = auditScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await auditDb.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS "AuditLogs" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_AuditLogs" PRIMARY KEY AUTOINCREMENT,
            "UserId" INTEGER NULL,
            "Username" TEXT NULL,
            "Role" TEXT NULL,
            "Action" TEXT NOT NULL,
            "Entity" TEXT NULL,
            "HttpMethod" TEXT NOT NULL,
            "Path" TEXT NOT NULL,
            "StatusCode" INTEGER NOT NULL,
            "IpAddress" TEXT NULL,
            "CreatedAt" TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS "IX_AuditLogs_CreatedAt" ON "AuditLogs" ("CreatedAt");
        CREATE INDEX IF NOT EXISTS "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
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
app.UseMiddleware<BackEnd.Middleware.AuditMiddleware>();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();
