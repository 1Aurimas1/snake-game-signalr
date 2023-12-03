using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SnakeGame.Server.Auth.Services.TokenServices;
using SnakeGame.Server.Filters;
using SnakeGame.Server.Helpers;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthApi(this RouteGroupBuilder group)
    {
        group
            .MapPost("/register", Register)
            .WithName(nameof(Register))
            .AddEndpointFilter<ValidationFilter<RegisterUserDto>>();
        group.MapPost("/login", Login).AddEndpointFilter<ValidationFilter<LoginUserDto>>();
        // TODO: validation?
        group.MapPost("/logout", Logout);
        group
            .MapPost("/change-password", ChangePassword)
            .AddEndpointFilter<ValidationFilter<ChangePasswordDto>>();
        group.MapPost("/accessToken", RefreshAccessToken);

        return group;
    }

    public static async Task<IResult> Register(RegisterUserDto dto, IUserService userService)
    {
        var user = dto.FromRegisterDto();

        var identityErrors = await userService.Register(user, dto.Password);
        if (identityErrors != null)
            return Results.UnprocessableEntity(
                JsonResponseGenerator.GenerateIdentityErrorResponse(identityErrors)
            );

        return Results.CreatedAtRoute(nameof(Register), user.ToDto());
    }

    public static async Task<IResult> Login(
        LoginUserDto dto,
        IUserService userService,
        IJwtTokenService jwtTokenService
    )
    {
        var result = await userService.Login(dto.UserName, dto.Password);
        if (result == null)
            return Results.NotFound(
                JsonResponseGenerator.GenerateNotFoundResponse("Username or password was incorrect")
            );

        (var user, var roles) = result.Value;

        await jwtTokenService.RemoveExpiredUserTokens(user.Id);
        await userService.UpdateForceRelogin(user, false);

        var accessToken = jwtTokenService.CreateAccessToken(
            user.UserName,
            user.Id.ToString(),
            roles
        );
        var refreshToken = jwtTokenService.CreateRefreshToken(user.Id.ToString());

        return Results.Ok(new SuccessfulLoginDto(accessToken, refreshToken));
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> Logout(
        RefreshAccessTokenDto dto,
        HttpContext httpContext,
        IJwtTokenService jwtTokenService,
        IUserService userService
    )
    {
        if (!httpContext.TryGetJwtUserId(out int userId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "userId",
                    "Invalid user ID"
                )
            );

        if (!jwtTokenService.TryParseRefreshToken(dto.RefreshToken, out var claims))
        {
            return Results.UnprocessableEntity("Couldn't parse refresh token");
        }

        var user = await userService.Get(userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var expiration = claims.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (expiration == null)
            return TypedResults.UnprocessableEntity();

        var revokedToken = await jwtTokenService.AddTokenToBlacklist(
            dto.RefreshToken,
            userId,
            expiration
        );
        if (revokedToken == null)
            return TypedResults.UnprocessableEntity();

        await userService.UpdateForceRelogin(user, true);

        return Results.Ok();
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> ChangePassword(
        ChangePasswordDto dto,
        HttpContext httpContext,
        IUserService userService
    )
    {
        if (!httpContext.TryGetJwtUserId(out int userId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "userId",
                    "Invalid user ID"
                )
            );

        var user = await userService.Get(userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var changePasswordResult = await userService.ChangePassword(
            user,
            dto.OldPassword,
            dto.NewPassword
        );
        if (!changePasswordResult.Succeeded)
            return TypedResults.UnprocessableEntity("Password change failed");

        return Results.Ok();
    }

    public static async Task<IResult> RefreshAccessToken(
        RefreshAccessTokenDto dto,
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService
    )
    {
        if (!jwtTokenService.TryParseRefreshToken(dto.RefreshToken, out var claims))
        {
            return Results.UnprocessableEntity("Couldn't parse refresh token");
        }

        if (await jwtTokenService.IsTokenBlacklisted(dto.RefreshToken))
        {
            return Results.UnprocessableEntity("Invalid token provided");
        }

        var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.UnprocessableEntity("Invalid token");
        }

        if (user.ForceRelogin)
        {
            return Results.UnprocessableEntity("Relogin required");
        }

        var expiration = claims.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (expiration == null)
            return TypedResults.UnprocessableEntity();
        var revokedToken = await jwtTokenService.AddTokenToBlacklist(
            dto.RefreshToken,
            user.Id,
            expiration
        );
        if (revokedToken == null)
            return TypedResults.UnprocessableEntity();

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.CreateAccessToken(
            user.UserName,
            user.Id.ToString(),
            roles
        );
        var refreshToken = jwtTokenService.CreateRefreshToken(user.Id.ToString());

        return Results.Ok(new SuccessfulLoginDto(accessToken, refreshToken));
    }
}
