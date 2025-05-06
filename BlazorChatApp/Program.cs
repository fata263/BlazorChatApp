using BlazorChatApp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using BlazorChatApp.services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

builder.Services.AddHttpClient("ServerAPI", client =>
{
    var baseUrl = builder.Configuration["BaseUrl"] ?? "https://localhost:5001/";
    client.BaseAddress = new Uri(baseUrl);
});
// ✅ Provide a default HttpClient via DI
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));

builder.Services.AddSignalR();
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));
builder.Services.AddControllers();

builder.Services.AddScoped<SignalRService>();      // Per user SignalR conn
builder.Services.AddScoped<UserService>();         // Per user
builder.Services.AddScoped<AppStatusService>();    // Safe access to DbContext

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