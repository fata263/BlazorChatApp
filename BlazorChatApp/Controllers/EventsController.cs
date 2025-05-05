using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorChatApp.Data;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ChatDbContext _context;

    public EventsController(ChatDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppEvent>> Get(int id)
    {
        var evt = await _context.Events.FindAsync(id);
        if (evt == null)
            return NotFound();

        return Ok(evt);
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] AppEvent updated)
    {
        var evt = await _context.Events.FindAsync(updated.Id);
        if (evt == null)
            return NotFound();

        evt.State = updated.State;
        evt.JsonData = updated.JsonData;
        evt.ForwardedTo = updated.ForwardedTo;
        evt.Timestamp = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(evt);
    }
}