using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using SnakeGame.Server.Filters;
using SnakeGame.Server.Helpers;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersApi(this RouteGroupBuilder group)
    {
        group.MapGet("/users", GetAllUsers);
        group.MapGet("/users/{id}", GetUser);
        group
            .MapPatch("/users/{id}", UpdateUser)
            .AddEndpointFilter<ValidationFilter<UpdateUserDto>>();
        group.MapDelete("/users/{id}", RemoveUser);

        return group;
    }

    [Authorize(Roles = UserRoles.Admin)]
    public static async Task<Ok<List<PrivateUserDto>>> GetAllUsers(IUserService userService)
    {
        var users = await userService.GetAll();

        return TypedResults.Ok(users.Select(x => x.ToPrivateDto()).ToList());
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<
        Results<Ok<PrivateUserDto>, ForbidHttpResult, NotFound<CustomError>>
    > GetUser(int id, HttpContext httpContext, IUserService userService)
    {
        if (!httpContext.CanUserAccessEndpoint(id))
        {
            return TypedResults.Forbid();
        }

        var user = await userService.Get(id);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        return TypedResults.Ok(user.ToPrivateDto());
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<
        Results<
            Ok<PrivateUserDto>,
            UnprocessableEntity<CustomError>,
            ForbidHttpResult,
            NotFound<CustomError>
        >
    > UpdateUser(int id, HttpContext httpContext, UpdateUserDto dto, IUserService userService)
    {
        if (!httpContext.TryGetJwtUserId(out int jwtId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "UserId",
                    "Invalid user ID"
                )
            );

        if (!httpContext.CanUserAccessEndpoint(id))
        {
            return TypedResults.Forbid();
        }

        var user = await userService.Get(jwtId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var updateResult = await userService.Update(user, dto);
        if (!updateResult.Succeeded)
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse("", "Update error")
            );

        return TypedResults.Ok(user.ToPrivateDto());
    }

    [Authorize(Roles = UserRoles.Admin)]
    public static async Task<
        Results<NoContent, NotFound<CustomError>, UnprocessableEntity<string>>
    > RemoveUser(int id, IUserService userService)
    {
        var user = await userService.Get(id);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var deleteResult = await userService.Remove(user);
        if (!deleteResult.Succeeded)
            return TypedResults.UnprocessableEntity("Delete error");

        return TypedResults.NoContent();
    }
}
