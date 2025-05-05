using BlazorChatApp.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using BlazorChatApp.services;

public class ChatHub(ChatDbContext context, AppStatusService statusService, UserService userService) : Hub
{
    private static readonly Dictionary<string, string> UserConnections = new();
    private static readonly Dictionary<string, int> UserStations = new();

    public async Task Register(string username, int stationId)
    {
        try
        {
            UserConnections[Context.ConnectionId] = username;
            UserStations[Context.ConnectionId] = stationId;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"Station-{stationId}");
            await NotifyActiveUsers();
        }
        catch (Exception ex)
        {
            statusService.ReportError($"⚠️ Register error: {ex.Message}");
        }
    }

    //public async Task<List<UserDto>> GetActiveUsers()
    //{
    //    try
    //    {
    //        var usernames = UserConnections.Values.Distinct().ToList();

    //        var users = await context.Users
    //            .Include(u => u.Station)
    //            .Where(u => usernames.Contains(u.Username))
    //            .ToListAsync();

    //        return users.Select(u => new UserDto
    //        {
    //            Username = u.Username,
    //            FirstName = u.FirstName,
    //            LastName = u.LastName,
    //            Role = u.Role.ToString(),
    //            SupervisorId = u.SupervisorId ?? 0,
    //            StationName = u.Station?.Name ?? "(Unknown)"
    //        }).ToList();
    //    }
    //    catch (Exception ex)
    //    {
    //        statusService.ReportError($"⚠️ GetActiveUsers error: {ex.Message}");
    //        return new();
    //    }
    //}

    public async Task<List<AppEvent>> GetRecentEvents()
    {
        try
        {
            return await context.Events
                .OrderByDescending(e => e.Timestamp)
                .Take(20)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            statusService.ReportError($"⚠️ GetRecentEvents error: {ex.Message}");
            return new();
        }
    }

    public async Task SendEvent(AppEvent evt, string? targetUser, string scope)
    {
        try
        {
            evt.Timestamp = DateTime.UtcNow;
            context.Events.Add(evt);
            await context.SaveChangesAsync();

            if (scope == "All")
            {
                await Clients.All.SendAsync("ReceiveEvent", evt);
            }
            else if (scope == "Station" && evt.StationId.HasValue)
            {
                await Clients.Group($"Station-{evt.StationId.Value}").SendAsync("ReceiveEvent", evt);
            }
            else if (scope == "User" && !string.IsNullOrEmpty(targetUser))
            {
                var connIds = UserConnections
                    .Where(kv => kv.Value == targetUser)
                    .Select(kv => kv.Key);

                foreach (var connId in connIds)
                    await Clients.Client(connId).SendAsync("ReceiveEvent", evt);
            }
        }
        catch (Exception ex)
        {
            statusService.ReportError($"⚠️ SendEvent error: {ex.Message}");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext()?.Request.Query["username"].ToString();
        var stationIdStr = Context.GetHttpContext()?.Request.Query["stationId"];
        var stationId = int.TryParse(stationIdStr, out var id) ? id : 0;
        UserConnections[Context.ConnectionId] = username;

        if (!string.IsNullOrEmpty(username))
        {
            userService.RegisterConnection(username, Context.ConnectionId);
            await NotifyActiveUsers();  
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        UserConnections.Remove(Context.ConnectionId);
        UserStations.Remove(Context.ConnectionId);
        await NotifyActiveUsers();
        await base.OnDisconnectedAsync(exception);
    }

    public async Task PromoteEvent(int eventId, EventState nextState)
    {
        var evt = await context.Events.FindAsync(eventId);
        if (evt == null) return;

        evt.State = nextState;
        await context.SaveChangesAsync();

        // Optionally broadcast update
        await Clients.All.SendAsync("EventUpdated", evt);
    }

    public async Task NotifyActiveUsers()
    {
        try
        {
            var users = await userService.GetActiveUsers();
            await Clients.All.SendAsync("UserListUpdated", users); 
        }
        catch (Exception ex)
        {
            statusService.ReportError("Failed to notify active users: " + ex.Message);
        }
    }

}
