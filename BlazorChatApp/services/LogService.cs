using System;

namespace BlazorChatApp.services
{
    public class LogService
    {
        public event Action<string>? OnLog;
        public void Write(string message) => OnLog?.Invoke(message);
    }
}
