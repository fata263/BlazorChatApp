using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorChatApp.Data;

public class Station
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    [JsonIgnore]
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}