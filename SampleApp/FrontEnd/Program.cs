using FrontEnd.Data;
using FrontEnd.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();


// =========================
// Authentication Service
// =========================

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ThemeService>();


// =========================
// Backend API
// =========================

builder.Services.AddHttpClient("Backend", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["BackendUrl"]
        ?? "http://localhost:8080"
    );
});


// =========================
// Weather API
// =========================

builder.Services.AddHttpClient<WeatherForecastClient>(c =>
{
    var url = builder.Configuration["WEATHER_URL"]
        ?? throw new InvalidOperationException(
            "WEATHER_URL is not set"
        );

    c.BaseAddress = new(url);
});


var app = builder.Build();


// =========================
// HTTP Pipeline
// =========================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();

app.MapFallbackToPage("/_Host");

app.Run();