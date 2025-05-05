namespace BlazorChatApp.Data
{
    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public int? StationId { get; set; }
    }

}
