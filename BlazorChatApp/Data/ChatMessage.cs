using System;

namespace BlazorChatApp.Data;

public class ChatMessage
{
    public int Id { get; set; }
    public string Sender { get; set; } = "";
    public string Receiver { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}