using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorChatApp.Data
{
    public class AppLog
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = "system";

        [Required]
        public string Message { get; set; } = "";

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public LogLevel Level { get; set; } = LogLevel.Info;
    }

    public enum LogLevel
    {
        Info = 0,
        Error = 1,
        Critical = 2
    }

}
