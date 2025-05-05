using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorChatApp.Data;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    public UserRole Role { get; set; }

    public int? SupervisorId { get; set; }
    public AppUser? Supervisor { get; set; }
    public int? StationId { get; set; }

    [ForeignKey("StationId")]
    public Station? Station { get; set; }
    public ICollection<AppUser>? Subordinates { get; set; }
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

}

public enum UserRole
{
    Operator = 1,
    Supervisor = 2,
    TeamLead = 3,
    IT = 4,
    Manager = 5
}
