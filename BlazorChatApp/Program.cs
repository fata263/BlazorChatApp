using BlazorChatApp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using BlazorChatApp.services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Serilog;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);

builder.Services.AddHttpClient("ServerAPI", client =>
{
    var baseUrl = Environment.GetEnvironmentVariable("BaseUrl") ?? builder.Configuration["BaseUrl"] ?? "http://localhost:5001";
    Console.WriteLine("Base URL: " + baseUrl);
    client.BaseAddress = new Uri(baseUrl);
});
// ✅ Provide a default HttpClient via DI
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));

builder.Services.AddSignalR();
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));
builder.Services.AddControllers();

builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AppStatusService>();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
        .WriteTo.Console();
});
builder.Logging.AddConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug); // Change from Information to Debug
builder.Logging.AddFilter("Microsoft.AspNetCore", Microsoft.Extensions.Logging.LogLevel.Debug); // Detailed ASP.NET Core logs
builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", Microsoft.Extensions.Logging.LogLevel.Debug); // SignalR logs
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", Microsoft.Extensions.Logging.LogLevel.Debug); // SignalR connection logs
builder.Logging.AddFilter("BlazorChatApp", Microsoft.Extensions.Logging.LogLevel.Debug); // Custom logs for your app namespace

var portStr = Environment.GetEnvironmentVariable("ListeningPort") ?? "5001";
int.TryParse(portStr, out var port);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port); // HTTP only
});

var app = builder.Build();

app.Logger.LogInformation("Application starting up...");

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting up...");
logger.LogDebug("Environment variables: {EnvVars}", Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().ToDictionary(e => e.Key, e => e.Value));

try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        logger.LogInformation("Initializing database...");
        DbInitializer.Initialize(services);
        logger.LogInformation("Database initialized successfully.");
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to initialize database.");
    throw; // Rethrow to ensure the app fails and logs are captured
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogDebug("Request Path: {Path}, Method: {Method}", context.Request.Path, context.Request.Method);
    await next(context);
    logger.LogDebug("Response Status Code: {StatusCode}", context.Response.StatusCode);
});

app.MapBlazorHub();
app.MapHub<ChatHub>("/chathub", options =>
{
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
});
app.MapGet("/health", () => "Healthy");
app.MapFallbackToPage("/_Host");

logger.LogInformation("Application startup completed. Listening on port 5001...");
app.Run();