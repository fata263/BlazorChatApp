using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BlazorChatApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace BlazorChatApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationsController(ChatDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Station>>> GetStations()
    {
        try
        {
            var stations = await context.Stations.ToListAsync();
            return Ok(stations);
        }
        catch (Exception ex)
        {
            Console.WriteLine("API /api/stations error: " + ex.Message);
            return StatusCode(500, "Station loading failed: " + ex.Message);
        }
    }
}