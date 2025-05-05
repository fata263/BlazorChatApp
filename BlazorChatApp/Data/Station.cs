using System.Collections.Generic;

namespace BlazorChatApp.Data;

public class Station
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}