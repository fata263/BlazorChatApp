using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorChatApp.Data;

public static class DbInitializer
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new ChatDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ChatDbContext>>());

        context.Database.EnsureCreated();

        if (!context.Stations.Any())
        {
            context.Stations.AddRange(
                new Station { Name = "Station A" },
                new Station { Name = "Station B" },
                new Station { Name = "Station C" }
            );

            context.SaveChanges();
        }


        context.SaveChanges();
    }
}