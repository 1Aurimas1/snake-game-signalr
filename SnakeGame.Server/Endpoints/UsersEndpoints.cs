using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersApi(this RouteGroupBuilder group)
    {
        group.MapGet("/users", GetAllUsers);
        group.MapGet("/users/{id}", GetUser);
        group.MapPatch("/users/{id}", UpdateUser);
        group.MapDelete("/users/{id}", RemoveUser);

        return group;
    }

    public static async Task<Ok<List<UserDto>>> GetAllUsers(IUserService userService)
    {
        var users = await userService.GetAll();

        return TypedResults.Ok(users.Select(x => x.ToDto()).ToList());
    }

    public static async Task<Results<Ok<UserDto>, NotFound<CustomError>>> GetUser(
        int id,
        IUserService userService
    )
    {
        var user = await userService.Get(id);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        return TypedResults.Ok(user.ToDto());
    }

    public static async Task<
        Results<
            Ok<UserDto>,
            UnprocessableEntity<IEnumerable<CustomError>>,
            UnprocessableEntity<string>,
            NotFound<CustomError>
        >
    > UpdateUser(
        int id,
        UpdateUserDto dto,
        IValidator<UpdateUserDto> validator,
        IUserService userService
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return TypedResults.UnprocessableEntity(responseResult);
        }

        var user = await userService.Get(id);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var updateResult = await userService.Update(user, dto);
        if (!updateResult.Succeeded)
            return TypedResults.UnprocessableEntity("Update error");

        return TypedResults.Ok(user.ToDto());
    }

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
