using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SnakeGame.Server.Models;

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

    public static async Task<IResult> GetAllUsers(DataContext dbContext)
    {
        var users = await dbContext.Users.ToListAsync();

        return Results.Ok(users.Select(x => x.ToDto()));
    }

    public static async Task<IResult> GetUser(string id, UserManager<User> userManager)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        return Results.Ok(user.ToDto());
    }

    public static async Task<IResult> UpdateUser(
        string id,
        UpdateUserDto dto,
        IValidator<UpdateUserDto> validator,
        UserManager<User> userManager
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        user.UpdateWithDto(dto);

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Results.UnprocessableEntity("Update error");

        return Results.Ok(user.ToDto());
    }

    public static async Task<IResult> RemoveUser(string id, UserManager<User> userManager)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var deleteResult = await userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
            return Results.UnprocessableEntity("Delete error");

        return Results.NoContent();
    }
}
