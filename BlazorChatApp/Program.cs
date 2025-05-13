using BlazorChatApp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using BlazorChatApp.services;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

var baseUrl = Environment.GetEnvironmentVariable("BaseUrl") ?? builder.Configuration["BaseUrl"] ?? "http://localhost:64779";
Console.WriteLine("Base URL: " + baseUrl);

builder.Services.AddHttpClient("ServerAPI", client =>
{
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

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // HTTP only
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    DbInitializer.Initialize(services);
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.MapBlazorHub();
app.MapHub<ChatHub>("/chathub");
app.MapFallbackToPage("/_Host");

app.Run();