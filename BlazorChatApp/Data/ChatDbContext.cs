using Microsoft.EntityFrameworkCore;

namespace BlazorChatApp.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<ChatMessage> Messages => Set<ChatMessage>(); 
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<AppEvent> Events => Set<AppEvent>();
    public DbSet<AppLog> SystemLogs => Set<AppLog>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Supervisor)
            .WithMany(s => s.Subordinates)
            .HasForeignKey(u => u.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}