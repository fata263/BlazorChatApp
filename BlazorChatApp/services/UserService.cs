using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorChatApp.Data;
using BlazorChatApp.services;
using Microsoft.EntityFrameworkCore;

public class UserService(ChatDbContext context, AppStatusService statusService)
{
    private static readonly Dictionary<string, string> UserConnections = new(); // username -> connectionId
    public AppUser? CurrentUser { get; private set; }
    public string CurrentStationName { get; private set; } = "";

    public void SetUser(AppUser user)
    {
        CurrentUser = user;
    }

    public void SetStationName(string stationName)
    {
        CurrentStationName = stationName;
    }

    public void Clear()
    {
        CurrentUser = null;
        CurrentStationName = "";
    }

    public void RegisterConnection(string username, string connectionId)
    {
        UserConnections[username] = connectionId;
    }

    public void RemoveConnection(string connectionId)
    {
        var entry = UserConnections.FirstOrDefault(kvp => kvp.Value == connectionId);
        if (!string.IsNullOrEmpty(entry.Key))
            UserConnections.Remove(entry.Key);
    }

    public async Task<List<AppUser>> GetActiveUsers()
    {
        try
        {
            var usernames = UserConnections.Keys.ToList();

            var users = await context.Users
                .Where(u => usernames.Contains(u.Username))
                .ToListAsync();

            return users;
        }
        catch (Exception ex)
        {
            statusService.ReportError("⚠️ GetActiveUsers failed: " + ex.Message);
            return new();
        }
    }
}