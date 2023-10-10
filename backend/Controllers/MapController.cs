using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using snake_game.Models;

namespace snake_game.Controllers;

[Authorize]
[Route("/api/[controller]")]
[ApiController]
public class MapController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ITokenManager _tokenManager;
    private readonly DataContext _context;

    public MapController(IConfiguration configuration, ITokenManager tokenManager, DataContext context)
    {
        _configuration = configuration;
        _tokenManager = tokenManager;
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<string>> Create(MapDto)
    {
        _context.Users.Add(new User(userDto));
        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }
}
