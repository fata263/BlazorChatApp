using BlazorChatApp.Data;
using System;

namespace BlazorChatApp.services
{

    public class AppStatusService(ChatDbContext context)
    {
        public event Action<string>? OnError;

        public void Report(string message, LogLevel level = LogLevel.Info, string username = "system")
        {
            SaveToDb(username, message, level);
            if (level >= LogLevel.Error)
            {
                OnError?.Invoke($"[{username}] ⚠️ {message}");
            }
        }

        public string LatestMessage { get; private set; } = "🔄 System ready.";

        public void ReportInfo(string message)
        {
            LatestMessage = message;
        }

        public void ReportError(string message)
        {
            LatestMessage = $"❗ {message}";
            Report(message, LogLevel.Error);
        }

        public void ReportError(string username, string message) 
        {
            LatestMessage = $"❗ {message}";
            Report(message, LogLevel.Error, username);
        }

        private void SaveToDb(string user, string message, LogLevel level)
        {
            try
            {
                context.SystemLogs.Add(new AppLog
                {
                    Username = user,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    Level = level
                });
                context.SaveChanges();
            }
            catch
            {
                Console.WriteLine("Catastrophic failure!!!");
            }
        }
    }


}
