using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SnakeGame.Server.Models;

namespace SnakeGame.Server.Helpers;

public static class HttpContextExtensions
{
    public static bool TryGetJwtUserId(this HttpContext httpContext, out int userId)
    {
        userId = 0;
        var userClaimsId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userClaimsId == null)
            return false;

        if (!int.TryParse(userClaimsId, out userId))
            return false;

        return true;
    }

    public static bool CanUserAccessEndpoint(this HttpContext httpContext, int resourceOwnerId)
    {
        if (
            httpContext.User.IsInRole(UserRoles.Admin)
            || httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) == resourceOwnerId.ToString()
        )
            return true;

        return false;
    }
}
