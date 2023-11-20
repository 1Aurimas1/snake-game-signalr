using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SnakeGame.Server.Auth.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace SnakeGame.Server.Auth.Services.TokenServices;

public interface IJwtTokenService
{
    Task<RevokedJwtToken?> AddTokenToBlacklist(string refreshToken, int userId, string expiration);
    string CreateAccessToken(string username, string userId, IEnumerable<string> roles);
    string CreateRefreshToken(string userId);
    Task<bool> IsTokenBlacklisted(string refreshToken);
    Task RemoveExpiredUserTokens(int userId);
    bool TryParseRefreshToken(string refreshToken, out ClaimsPrincipal? claims);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly SymmetricSecurityKey _authSigningKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly DataContext _dbContext;

    public JwtTokenService(IConfiguration configuration, DataContext dbContext)
    {
        _authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
        );
        _issuer = configuration["Jwt:ValidIssuer"]!;
        _audience = configuration["Jwt:ValidAudience"]!;

        _dbContext = dbContext;
    }

    public string CreateAccessToken(string username, string userId, IEnumerable<string> roles)
    {
        var authClaims = new List<Claim>()
        {
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, userId)
        };

        authClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            expires: DateTime.UtcNow.AddMinutes(10),
            claims: authClaims,
            signingCredentials: new SigningCredentials(
                _authSigningKey,
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken(string userId)
    {
        var authClaims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, userId)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            expires: DateTime.UtcNow.AddHours(24),
            claims: authClaims,
            signingCredentials: new SigningCredentials(
                _authSigningKey,
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool TryParseRefreshToken(string refreshToken, out ClaimsPrincipal? claims)
    {
        claims = null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = _authSigningKey,
                ValidateLifetime = true,
            };

            claims = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<RevokedJwtToken?> AddTokenToBlacklist(
        string refreshToken,
        int userId,
        string expiration
    )
    {
        if (!long.TryParse(expiration, out var unixSeconds))
            return null;

        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

        var revokedToken = new RevokedJwtToken
        {
            UserId = userId,
            RefreshToken = refreshToken,
            ExpirationDate = dateTimeOffset.UtcDateTime,
        };

        await _dbContext.RevokedJwtTokens.AddAsync(revokedToken);
        await _dbContext.SaveChangesAsync();

        return revokedToken;
    }

    public async Task<bool> IsTokenBlacklisted(string refreshToken)
    {
        return await _dbContext.RevokedJwtTokens.AnyAsync(x => x.RefreshToken == refreshToken);
    }

    public async Task RemoveExpiredUserTokens(int userId)
    {
        var utcTimeNow = DateTime.UtcNow;
        var tokens = _dbContext.RevokedJwtTokens.Where(
            x => x.UserId == userId && x.ExpirationDate <= utcTimeNow
        );
        _dbContext.RevokedJwtTokens.RemoveRange(tokens);

        await _dbContext.SaveChangesAsync();
    }
}
