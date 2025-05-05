using BlazorChatApp.Data;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class SignalRService
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

        _hub.On<AppEvent>("ReceiveEvent", async evt =>
        {
            ReceivedEvents.Insert(0, evt);
            if (OnEventReceived != null)
                await OnEventReceived.Invoke(evt);
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

        
        await _hub.StartAsync();
    }

    public async Task SendEvent(AppEvent evt)
    {
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.SendAsync("SendEvent", evt);
    }

    public async Task StopAsync()
    {
        if (_hub != null && _hub.State == HubConnectionState.Connected)
            await _hub.StopAsync();
    }

    public async Task NotifyEventUpdated(AppEvent evt)
    {
        if (_hub?.State == HubConnectionState.Connected)
        {
            await _hub.SendAsync("UpdateEvent", evt);
        }
    }

}