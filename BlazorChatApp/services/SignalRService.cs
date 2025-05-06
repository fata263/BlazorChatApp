using BlazorChatApp.Data;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using BlazorChatApp.services;
using Microsoft.JSInterop;
using static System.Net.WebRequestMethods;
using System.Net.Http;
using System.Net.Http.Json;

public class SignalRService(AppStatusService statusService, IJSRuntime js, HttpClient http)
{
    private HubConnection? _hub;

    public List<AppEvent> ReceivedEvents { get; private set; } = new();

    public event Func<AppEvent, Task>? OnEventReceived;
    public event Func<AppEvent, Task>? OnEventUpdated;
    public event Action<List<AppUser>>? OnUserListUpdated;

    public async Task StartAsync(string baseUri, string username, int stationId)
    {
        if (_hub != null && _hub.State == HubConnectionState.Connected)
            return;

        _hub = new HubConnectionBuilder()
            .WithUrl($"{baseUri}chathub?username={username}&stationId={stationId}")
            .WithAutomaticReconnect()
            .Build();

        RegisterEventHandlers();

        try
        {
            await _hub.StartAsync();
            await LoadRecentEventsAsync();
        }
        catch (Exception ex)
        {
            statusService.ReportError("🔌 SignalR connection failed: " + ex.Message);
        }
    }

    private void RegisterEventHandlers()
    {
        _hub.On<AppEvent>("ReceiveEvent", async evt =>
        {
            if (ReceivedEvents.All(e => e.Id != evt.Id))
            {
                ReceivedEvents.Insert(0, evt);
                if (ReceivedEvents.Count > 50)
                    ReceivedEvents.RemoveAt(ReceivedEvents.Count - 1);
            }

            if (OnEventReceived != null)
                await OnEventReceived.Invoke(evt);
            try
            {
                await js.InvokeVoidAsync("startFlashingTitle", "🔔 New Event!");
            }
            catch { /* JS error suppressed */ }
        });

        _hub.On<AppEvent>("EventUpdated", async evt =>
        {
            var index = ReceivedEvents.FindIndex(e => e.Id == evt.Id);
            if (index >= 0)
                ReceivedEvents[index] = evt;
            else
                ReceivedEvents.Insert(0, evt);

            if (OnEventUpdated != null)
                await OnEventUpdated.Invoke(evt);
        });

        _hub.On<List<AppUser>>("UserListUpdated", users =>
        {
            OnUserListUpdated?.Invoke(users);
        });

        _hub.Reconnected += async (id) =>
        {
            statusService.ReportInfo("🔄 Reconnected. Syncing recent events...");
            await LoadRecentEventsAsync();
        };
    }

    public async Task SendEvent(AppEvent evt, string? targetUser, string scope)
    {
        if (_hub?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hub.SendAsync("SendEvent", evt, targetUser, scope);
            }
            catch (Exception ex)
            {
                statusService.ReportError("❗ Failed to send event: " + ex.Message);
            }
        }
    }

    public async Task NotifyEventUpdated(AppEvent evt)
    {
        if (_hub?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hub.SendAsync("UpdateEvent", evt);
            }
            catch (Exception ex)
            {
                statusService.ReportError("❗ Event update failed: " + ex.Message);
            }
        }
    }

    public async Task<List<AppEvent>> LoadRecentEventsAsync()
    {
        try
        {
            var recent = await http.GetFromJsonAsync<List<AppEvent>>("api/events/recent");
            if (recent != null)
            {
                ReceivedEvents = recent.OrderByDescending(e => e.Timestamp).Take(50).ToList();
            }

            return ReceivedEvents;
        }
        catch (Exception ex)
        {
            statusService.ReportError("⚠ Failed to load recent events: " + ex.Message);
            return new();
        }
    }

    public async Task StopAsync()
    {
        try
        {
            if (_hub != null && _hub.State == HubConnectionState.Connected)
                await _hub.StopAsync();
        }
        catch (Exception ex)
        {
            statusService.ReportError("⚠ Error stopping SignalR: " + ex.Message);
        }
    }
}
