using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using snake_game.Models;

namespace snake_game.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly DataContext _context;

    public AuthController(IConfiguration configuration, DataContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<string>> Register(UserRegisterDto userDto)
    {
        if (!IsValidEmail(userDto.Email))
            ModelState.AddModelError(nameof(userDto.Email), "The email is not valid.");

        if (_context.Users.Any(u => u.Username == userDto.Username))
            ModelState.AddModelError(nameof(userDto.Username), "The username is already in use.");

        if (_context.Users.Any(u => u.Email == userDto.Email))
            ModelState.AddModelError(nameof(userDto.Email), "The email is already in use.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: password hashing
        _context.Users.Add(new User(userDto));
        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(UserDto userDto)
    {
        var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
        if (u == null)
            ModelState.AddModelError(nameof(userDto.Username), "User not registered");
        else if (u.PasswordHash != userDto.Password)
            ModelState.AddModelError(nameof(userDto.Username), "Incorrect login information");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string token = CreateToken(u!);

        return Ok(token);
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    private bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith(".")) return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }
}
