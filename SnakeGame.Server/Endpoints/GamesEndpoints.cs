using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using SnakeGame.Server.Models;

public static class GamesEndpoints
{
    public static RouteGroupBuilder MapGamesApi(this RouteGroupBuilder group)
    {
        group.MapGet("/games", GetAllGames);
        group.MapGet("/games/open", GetAllOpenGames);
        group.MapGet("/users/{userId}/games", GetAllUserGames);
        group.MapGet("/users/{userId}/games/{id}", GetUserGame);
        group.MapPost("/users/{userId}/games", CreateUserGame);
        group.MapPatch("/users/{userId}/games/{id}", UpdateUserGame);
        group.MapDelete("/users/{userId}/games/{id}", RemoveUserGame);

        return group;
    }

    private static IQueryable<Game> WithIncludes(this IQueryable<Game> query)
    {
        return query.Include(x => x.Map).Include(x => x.Creator).Include(x => x.Players);
    }

    public static async Task<Ok<List<GameDto>>> GetAllGames(DataContext dbContext)
    {
        var games = await dbContext.Games.WithIncludes().ToListAsync();

        return TypedResults.Ok(games.Select(x => x.ToDto()).ToList());
    }

    public static async Task<Ok<List<GameDto>>> GetAllOpenGames(DataContext dbContext)
    {
        var games = await dbContext.Games.WithIncludes().Where(x => x.IsOpen).ToListAsync();

        return TypedResults.Ok(games.Select(x => x.ToDto()).ToList());
    }

    public static async Task<Results<Ok<List<GameDto>>, NotFound<CustomError>>> GetAllUserGames(
        int userId,
        DataContext dbContext
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var games = await dbContext.Games
            .WithIncludes()
            .Where(x => x.Creator.Id == userId)
            .ToListAsync();

        return TypedResults.Ok(games.Select(x => x.ToDto()).ToList());
    }

    public static async Task<Results<Ok<GameDto>, NotFound<CustomError>>> GetUserGame(
        int userId,
        int id,
        DataContext dbContext
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var game = await dbContext.Games
            .WithIncludes()
            .FirstOrDefaultAsync(x => x.Id == id && x.Creator.Id == userId);
        if (game == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("game"));

        return TypedResults.Ok(game.ToDto());
    }

    public static async Task<
        Results<
            Created<GameDto>,
            UnprocessableEntity<IEnumerable<CustomError>>,
            NotFound<CustomError>
        >
    > CreateUserGame(
        int userId,
        CreateGameDto dto,
        IValidator<CreateGameDto> validator,
        DataContext dbContext
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return TypedResults.UnprocessableEntity(responseResult);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await dbContext.Maps
            .Include(x => x.Creator)
            .Include(x => x.MapObstacles)
            .ThenInclude(x => x.Position)
            .FirstOrDefaultAsync(x => x.Id == dto.MapId);
        if (map == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        var game = dto.FromCreateDto(map, user);
        dbContext.Games.Add(game);

        await dbContext.SaveChangesAsync();

        return TypedResults.Created($"/users/{userId}/games/{game.Id}", game.ToDto());
    }

    public static async Task<
        Results<
            Ok<GameDto>,
            UnprocessableEntity<IEnumerable<CustomError>>,
            UnprocessableEntity<CustomError>,
            NotFound<CustomError>
        >
    > UpdateUserGame(
        int userId,
        int id,
        UpdateGameDto dto,
        IValidator<UpdateGameDto> validator,
        DataContext dbContext
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var response = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return TypedResults.UnprocessableEntity(response);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var game = await dbContext.Games
            .WithIncludes()
            .FirstOrDefaultAsync(x => x.Id == id && x.Creator.Id == userId);
        if (game == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("game"));
        else if (!game.IsOpen)
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "player",
                    "Game is closed"
                )
            );

        var player = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == dto.PlayerId);
        if (player == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("player"));
        else if (game.Players.Exists(x => x.Id == dto.PlayerId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "player",
                    "Cannot join same game more than once"
                )
            );

        game.IsOpen = game.Mode switch
        {
            GameMode.Duel or GameMode.Solo => false,
            _ => throw new NotImplementedException("Add new GameMode enum members"),
        };

        game.Players.Add(player);

        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(game.ToDto());
    }

    public static async Task<Results<NoContent, NotFound<CustomError>>> RemoveUserGame(
        int userId,
        int id,
        DataContext dbContext
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var game = await dbContext.Games.FirstOrDefaultAsync(
            x => x.Id == id && x.Creator.Id == userId
        );
        if (game == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("game"));

        dbContext.Games.Remove(game);

        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
