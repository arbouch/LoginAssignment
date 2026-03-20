using System.Diagnostics;
using System.Net.Http.Headers;
using Frontend.Blazor;
using Frontend.Blazor.Components;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<AuthTokenStore>();

builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7100");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
     ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("An error occurred.");
        });
    });
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Open the login page in the default browser when you start from the terminal
// (`dotnet run --launch-profile http`). launchBrowser in launchSettings is not always applied.
if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            var server = app.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
            var baseUrl = addresses?
                .Select(static a => a
                    .Replace("0.0.0.0", "localhost", StringComparison.Ordinal)
                    .Replace("[::]", "localhost", StringComparison.Ordinal))
                .FirstOrDefault(static a => a.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                ?? addresses?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = "https://localhost:7200";

            var loginUrl = $"{baseUrl.TrimEnd('/')}/login";
            Process.Start(new ProcessStartInfo { FileName = loginUrl, UseShellExecute = true });
        }
        catch
        {
            // Ignore if no default browser / platform limitation
        }
    });
}

app.Run();
