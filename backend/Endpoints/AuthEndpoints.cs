using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using snake_game.Models;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthApi(this RouteGroupBuilder group)
    {
        group.MapPost("/auth/logout", Logout);
        group.MapPost("/auth/login", Login);

        return group;
    }

    private static IQueryable<Map> GetCompleteQuery(DataContext dbContext)
    {
        return dbContext.Maps
            .Include(x => x.Creator)
            .Include(x => x.MapObstacles)
            .ThenInclude(x => x.Position)
            .AsQueryable();
    }

    public static async Task<IResult> Logout()
    {
        // TODO: implement logout logic
        //_tokenManager.DeactivateCurrent();
        return Results.Ok();
    }

    public static async Task<IResult> Login(
        LoginUserDto dto,
        IValidator<LoginUserDto> validator,
        DataContext dbContext,
        IConfiguration configuration
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var hasher = new PasswordHasher<LoginUserDto>();
        var verificationResult = hasher.VerifyHashedPassword(dto, user.PasswordHash, dto.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Results.Unauthorized();
        }
        else if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            System.Console.WriteLine("Deprecated algorithm detected. Starting password rehash...");

            var newHash = hasher.HashPassword(dto, dto.Password);
            user.PasswordHash = newHash;

            await dbContext.SaveChangesAsync();

            System.Console.WriteLine("Password rehash completed!");
        }

        string token = CreateToken(user, configuration);

        return Results.Ok(token);
    }

    private static string CreateToken(User user, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds,
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"]
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
}
