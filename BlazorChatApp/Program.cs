using System;
using System.Net.Http;
using BlazorChatApp.Data;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Azure.Storage.Blobs;
using BlazorChatApp.services;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages(); // For _Host.cshtml
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddHealthChecks();

builder.Services.AddHttpClient("ServerAPI", client =>
{
    var baseUrl = Environment.GetEnvironmentVariable("BaseUrl") ?? builder.Configuration["BaseUrl"] ?? "http://localhost:5001";
    Console.WriteLine("Base URL: " + baseUrl);
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));

builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AppStatusService>();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.WriteTo.Console();
});
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug); // Change from Information to Debug
builder.Logging.AddFilter("Microsoft.AspNetCore", Microsoft.Extensions.Logging.LogLevel.Debug); // Detailed ASP.NET Core logs
builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", Microsoft.Extensions.Logging.LogLevel.Debug); // SignalR logs
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", Microsoft.Extensions.Logging.LogLevel.Debug); // SignalR connection logs
builder.Logging.AddFilter("BlazorChatApp", Microsoft.Extensions.Logging.LogLevel.Debug); // Custom logs for your app namespace

var portStr = Environment.GetEnvironmentVariable("ListeningPort") ?? "5001";
int.TryParse(portStr, out var port);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port);
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.Logger.LogInformation("Application starting up...");

try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Initializing database...");
    DbInitializer.Initialize(services);
    logger.LogInformation("Database initialized successfully.");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to initialize database.");
    throw; // Rethrow to ensure the app fails and logs are captured
}

app.UseStaticFiles(); // ✅ MUST come BEFORE routing

app.UseRouting();

app.MapControllers();
//app.UseAntiforgery();
//app.UseAuthentication();
//app.UseAuthorization();

// ✅ Blazor Server mapping:
app.MapBlazorHub();


app.MapHub<ChatHub>("/chathub");
app.UseHealthChecks("/health");

app.MapFallbackToPage("/_Host");

app.Logger.LogInformation("Application startup completed. Listening on port 5001...");

app.Run();