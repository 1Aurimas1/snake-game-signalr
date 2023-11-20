using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.TokenServices;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthApi(this RouteGroupBuilder group)
    {
        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/logout", Logout);
        group.MapPost("/accessToken", RefreshAccessToken);

        return group;
    }

    public static async Task<IResult> Register(
        UserManager<User> userManager,
        RegisterUserDto dto,
        IValidator<RegisterUserDto> validator
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

        var user = dto.FromRegisterDto();

        var userResult = await userManager.CreateAsync(user, dto.Password);
        // TODO: userResult.errors
        if (!userResult.Succeeded)
            return Results.UnprocessableEntity();

        // TODO: check result
        await userManager.AddToRoleAsync(user, UserRoles.Basic);

        return Results.Created("/register", user.ToDto());
    }

    public static async Task<IResult> Logout()
    {
        // TODO: implement logout logic
        //_tokenManager.DeactivateCurrent();
        return Results.Ok();
    }

    public static async Task<IResult> Login(
        UserManager<User> userManager,
        LoginUserDto dto,
        IValidator<LoginUserDto> validator,
        JwtTokenService jwtTokenService,
        IConfiguration configuration
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

        var user = await userManager.FindByNameAsync(dto.UserName);
        if (user == null)
            // TODO: unprocessable(username or p was incorect)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var isPasswordValid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            // TODO: unprocessable(username or p was incorect)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        user.ForceRelogin = false;
        await userManager.UpdateAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.CreateAccessToken(
            user.UserName,
            user.Id.ToString(),
            roles
        );
        var refreshToken = jwtTokenService.CreateRefreshToken(user.Id.ToString());

        return Results.Ok(new SuccessfulLoginDto(accessToken, refreshToken));
    }

    public static async Task<IResult> RefreshAccessToken(
        UserManager<User> userManager,
        JwtTokenService jwtTokenService,
        RefreshAccessTokenDto dto
    )
    {
        if (!jwtTokenService.TryParseRefreshToken(dto.RefreshToken, out var claims))
        {
            return Results.UnprocessableEntity("Couldn't parse refresh token");
        }

        var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.UnprocessableEntity("Invalid token");
        }

        if (user.ForceRelogin)
        {
            return Results.UnprocessableEntity();
        }

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
