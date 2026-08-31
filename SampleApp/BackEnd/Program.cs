using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));


// =========================
// JWT Authentication
// =========================

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT Key تنظیم نشده است");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer تنظیم نشده است");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience تنظیم نشده است");

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

builder.Services.AddAuthorization();


// =========================
// OpenAPI
// =========================

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Servers = [];

            var securitySchemes =
                new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        In = ParameterLocation.Header,
                        BearerFormat = "JWT"
                    }
                };

            document.Components ??= new OpenApiComponents();

            document.Components.SecuritySchemes =
                securitySchemes;

            return Task.CompletedTask;
        });

    options.AddOperationTransformer(
        (operation, context, cancellationToken) =>
        {
            var hasAuthorize =
                context.Description.ActionDescriptor
                    .EndpointMetadata
                    .OfType<Microsoft.AspNetCore.Authorization.IAuthorizeData>()
                    .Any();

            if (hasAuthorize)
            {
                operation.Security ??=
                    new List<OpenApiSecurityRequirement>();

                operation.Security.Add(
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(
                            "Bearer",
                            null!
                        )] = []
                    });
            }

            return Task.CompletedTask;
        });
});


var app = builder.Build();


// =========================
// HTTP Pipeline
// =========================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


// =========================
// Weather API
// =========================

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5)
        .Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(
                    DateTime.Now.AddDays(index)
                ),

                Random.Shared.Next(-20, 55),

                summaries[
                    Random.Shared.Next(
                        summaries.Length)
                ]
            ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();


internal record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary)
{
    public int TemperatureF =>
        32 + (int)(TemperatureC / 0.5556);
}