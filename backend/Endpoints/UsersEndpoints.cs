using FluentValidation;
using Microsoft.AspNetCore.Identity;
using snake_game.Models;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersApi(this RouteGroupBuilder group)
    {
        group.MapGet("/users", GetMany);
        group.MapGet("/users/{id}", Get);
        group.MapPatch("/users/{id}", Update);
        group.MapDelete("/users/{id}", Remove);

        return group;
    }

    public static async Task<IResult> GetMany(DataContext dbContext)
    {
        var users = await dbContext.Users.ToListAsync();

        return Results.Ok(users.Select(x => x.ToDto()));
    }

    public static async Task<IResult> Get(string id, UserManager<User> userManager)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        return Results.Ok(user.ToDto());
    }

    public static async Task<IResult> Update(
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

    public static async Task<IResult> Remove(string id, UserManager<User> userManager)
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
