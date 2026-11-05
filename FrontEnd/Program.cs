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

app.MapGet("/certificate-pdf/{certificateNumber}", async (string certificateNumber, IHttpClientFactory httpClientFactory, HttpContext context) => {     try     {         var client = httpClientFactory.CreateClient("Backend");         var response = await client.GetAsync("api/CertificateVerification/" + Uri.EscapeDataString(certificateNumber) + "/pdf");          if (!response.IsSuccessStatusCode)             return Results.StatusCode((int)response.StatusCode);          var bytes = await response.Content.ReadAsByteArrayAsync();         return Results.File(bytes, "application/pdf", certificateNumber + ".pdf");     }     catch (HttpRequestException)     {         return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "سرویس صدور گواهی در دسترس نیست.", detail: "لطفاً کمی بعد دوباره تلاش کنید.");     }     catch (TaskCanceledException) when (!context.RequestAborted.IsCancellationRequested)     {         return Results.Problem(statusCode: StatusCodes.Status504GatewayTimeout, title: "مهلت دریافت گواهی به پایان رسید.", detail: "لطفاً دوباره تلاش کنید.");     } });

app.MapBlazorHub();

app.MapFallbackToPage("/_Host");

app.Run();