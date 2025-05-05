using BlazorChatApp.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace BlazorChatApp.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController(ChatDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<AppUser>>> GetAll()
        {
            return await context.Users.Include(u => u.Supervisor).ToListAsync();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AppUser user)
        {
            if (context.Users.Any(u => u.Username.ToLower() == user.Username.ToLower()))
                return BadRequest("Username already exists.");

            var hasher = new PasswordHasher<AppUser>();
            user.Password = hasher.HashPassword(user, user.Password);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppUser>> Login([FromBody] Data.LoginRequest request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());
            if (user is null)
                return NotFound("Invalid user.");

            var hasher = new PasswordHasher<AppUser>();
            var result = hasher.VerifyHashedPassword(user, user.Password, request.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials.");

            if (request.StationId > 0)
            {
                user.StationId = request.StationId;
                await context.SaveChangesAsync();
            }

            return Ok(user);
        }

    }

}
