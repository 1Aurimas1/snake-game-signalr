using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using snake_game.Models;

namespace snake_game.Controllers;

[Authorize]
[Route("/api/[controller]")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly DataContext _context;

    public ProfileController(IConfiguration configuration, DataContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<Profile>> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return BadRequest("Error retrieving user id");

        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == int.Parse(userId));

        if (profile == null)
            return BadRequest("User profile not found");

        var dto = new ProfileDto(profile);

        return Ok(dto);
    }
}
