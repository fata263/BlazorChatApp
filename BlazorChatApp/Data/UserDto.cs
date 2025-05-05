namespace BlazorChatApp.Data
{
    public class UserDto
    {
        public string Username { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Role { get; set; } = "";
        public string StationName { get; set; } = "";
        public int? SupervisorId { get; set; } = 0;
    }
}
