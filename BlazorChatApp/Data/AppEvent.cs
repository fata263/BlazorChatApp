using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorChatApp.Data;

public class AppEvent
{
    public int Id { get; set; }
    [Required]
    public string Sender { get; set; } = string.Empty;
    public string? ForwardedTo { get; set; }
    public EventType Type { get; set; }
    public EventScope Scope { get; set; }
    public EventState State { get; set; } = EventState.New;
    public string JsonData { get; set; } = "{}";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int? StationId { get; set; }

    [ForeignKey("StationId")]
    public Station? Station { get; set; }
}

public enum EventState
{
    New,
    Active,
    Closed
}

public enum EventType
{
    Message,
    Incident,
    Alarm
}
public enum EventScope { All, Station, Private }