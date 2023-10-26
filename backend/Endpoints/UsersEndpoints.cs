using FluentValidation;
using Microsoft.AspNetCore.Identity;
using snake_game.Models;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersApi(this RouteGroupBuilder group)
    {
        group.MapGet("/users", GetMany);
        group.MapGet("/users/{id}", Get);
        group.MapPost("/users", Create);
        group.MapPatch("/users/{id}", Update);
        group.MapDelete("/users/{id}", Remove);

        return group;
    }

    public static async Task<IResult> GetMany(DataContext dbContext)
    {
        var users = await dbContext.Users.ToListAsync();

        return Results.Ok(users.Select(x => x.ToDto()));
    }

    public static async Task<IResult> Get(int id, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        return Results.Ok(user.ToDto());
    }

    public static async Task<IResult> Create(
        CreateUserDto dto,
        IValidator<CreateUserDto> validator,
        DataContext dbContext
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

        var hasher = new PasswordHasher<CreateUserDto>();
        string passwordHash = hasher.HashPassword(dto, dto.Password);

        var user = dto.FromCreateDto(passwordHash);
        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        return Results.Created($"/users/{user.Id}", user.ToDto());
    }

    public static async Task<IResult> Update(
        int id,
        UpdateUserDto dto,
        IValidator<UpdateUserDto> validator,
        DataContext dbContext
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        user.UpdateWithDto(dto);

        await dbContext.SaveChangesAsync();

        return Results.Ok(user.ToDto());
    }

    public static async Task<IResult> Remove(int id, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        dbContext.Users.Remove(user);

        await dbContext.SaveChangesAsync();

        return Results.NoContent();
    }
}
