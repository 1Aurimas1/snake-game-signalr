using FluentValidation;
using snake_game.Models;

public static class GamesEndpoints
{
    public static RouteGroupBuilder MapGamesApi(this RouteGroupBuilder group)
    {
        group.MapGet("/games", GetAll);
        group.MapGet("/games/open", GetAllOpen);
        group.MapGet("/users/{userId}/games", GetMany);
        group.MapGet("/users/{userId}/games/{id}", Get);
        group.MapPost("/users/{userId}/games", Create);
        group.MapPatch("/users/{userId}/games/{id}", Update);
        group.MapDelete("/users/{userId}/games/{id}", Remove);

        return group;
    }

    private static IQueryable<Game> GetCompleteQuery(DataContext dbContext)
    {
        return dbContext.Games
            .Include(x => x.Map)
            .Include(x => x.Creator)
            .Include(x => x.Players)
            .AsQueryable();
    }

    // Nonspecific endpoints

    public static async Task<IResult> GetAll(DataContext dbContext)
    {
        var games = await GetCompleteQuery(dbContext).ToListAsync();

        return Results.Ok(games.Select(x => x.ToDto()));
    }

    public static async Task<IResult> GetAllOpen(DataContext dbContext)
    {
        var games = await GetCompleteQuery(dbContext).Where(x => x.IsOpen).ToListAsync();

        return Results.Ok(games.Select(x => x.ToDto()));
    }

    // User specific endpoints

    public static async Task<IResult> GetMany(int userId, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var games = await GetCompleteQuery(dbContext)
            .Where(x => x.Creator.Id == userId)
            .ToListAsync();

        return Results.Ok(games.Select(x => x.ToDto()));
    }

    public static async Task<IResult> Get(int userId, int id, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var game = await GetCompleteQuery(dbContext)
            .FirstOrDefaultAsync(x => x.Id == id && x.Creator.Id == userId);
        if (game == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("game"));

        return Results.Ok(game.ToDto());
    }

    public static async Task<IResult> Create(
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
            return Results.UnprocessableEntity(responseResult);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await dbContext.Maps
            .Include(x => x.Creator)
            .Include(x => x.MapObstacles)
            .ThenInclude(x => x.Position)
            .FirstOrDefaultAsync(x => x.Id == dto.MapId);
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        var game = dto.FromCreateDto(map, user);
        dbContext.Games.Add(game);

        await dbContext.SaveChangesAsync();

        return Results.Created($"/users/{userId}/games/{game.Id}", game.ToDto());
    }

    public static async Task<IResult> Update(
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
            return Results.UnprocessableEntity(response);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var game = await GetCompleteQuery(dbContext)
            .FirstOrDefaultAsync(x => x.Id == id && x.Creator.Id == userId);
        if (game == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("game"));
        else if (!game.IsOpen)
            return Results.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "player",
                    "Game is closed"
                )
            );

        var player = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == dto.PlayerId);
        if (player == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("player"));
        else if (game.Players.Exists(x => x.Id == dto.PlayerId))
            return Results.UnprocessableEntity(
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

        return Results.Ok(game.ToDto());
    }

    public static async Task<IResult> Remove(int userId, int id, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var game = await dbContext.Games.FirstOrDefaultAsync(
            x => x.Id == id && x.Creator.Id == userId
        );
        if (game == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("game"));

        dbContext.Games.Remove(game);

        await dbContext.SaveChangesAsync();

        return Results.NoContent();
    }
}
