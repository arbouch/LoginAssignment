using System.Net.Http.Headers;
using Frontend.Blazor;
using Frontend.Blazor.Components;

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

app.Run();
