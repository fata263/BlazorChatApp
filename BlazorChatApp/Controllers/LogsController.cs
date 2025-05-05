using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorChatApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly ChatDbContext _context;

    public LogsController(ChatDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<AppLog>>> GetLogs()
    {
        try
        {
            var logs = await _context.SystemLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(100)
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Failed to load logs: " + ex.Message);
        }
    }
}