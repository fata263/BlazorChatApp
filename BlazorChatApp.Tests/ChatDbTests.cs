using System.Linq;
using Xunit;
using BlazorChatApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorChatApp.Tests;

public class ChatDbTests
{
    [Fact]
    public void CanStoreChatMessage()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        using var db = new ChatDbContext(options);
        db.Messages.Add(new ChatMessage { Sender = "Alice", Receiver = "Bob", Message = "Hi!" });
        db.SaveChanges();

        var msg = db.Messages.First();
        Assert.Equal("Alice", msg.Sender);
    }
}