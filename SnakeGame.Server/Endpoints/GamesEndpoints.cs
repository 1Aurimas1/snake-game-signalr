using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;

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

    public static async Task<Ok<List<GameDto>>> GetAllGames(IGameService gameService)
    {
        var games = await gameService.GetAll();

        return TypedResults.Ok(games.Select(x => x.ToDto()).ToList());
    }

    public static async Task<Ok<List<GameDto>>> GetAllOpenGames(IGameService gameService)
    {
        var games = await gameService.GetAllFiltered(g => g.IsOpen);

        return TypedResults.Ok(games.Select(x => x.ToDto()).ToList());
    }

    public static async Task<Results<Ok<List<GameDto>>, NotFound<CustomError>>> GetAllUserGames(
        int userId,
        IUserService userService,
        IGameService gameService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var games = await gameService.GetAllFiltered(g => g.Creator.Id == userId);

        return TypedResults.Ok(games.Select(x => x.ToDto()).ToList());
    }

    public static async Task<Results<Ok<GameDto>, NotFound<CustomError>>> GetUserGame(
        int userId,
        int id,
        IUserService userService,
        IGameService gameService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var game = await gameService.GetUserGame(userId, id);
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
        IUserService userService,
        IMapService mapService,
        IGameService gameService
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return TypedResults.UnprocessableEntity(responseResult);
        }

        var user = await userService.Get(userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await mapService.Get(dto.MapId);
        if (map == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        var game = dto.FromCreateDto(map, user);
        await gameService.Add(game);

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
        IUserService userService,
        IGameService gameService
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var response = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return TypedResults.UnprocessableEntity(response);
        }

        var user = await userService.Get(userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var game = await gameService.GetUserGame(userId, id);
        if (game == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("game"));
        else if (!game.IsOpen)
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "player",
                    "Game is closed"
                )
            );

        var player = await userService.Get(dto.PlayerId);
        if (player == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("player"));
        else if (game.Players.Exists(x => x.Id == dto.PlayerId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "player",
                    "Cannot join same game more than once"
                )
            );

        await gameService.Update(game, player);

        return TypedResults.Ok(game.ToDto());
    }

    public static async Task<Results<NoContent, NotFound<CustomError>>> RemoveUserGame(
        int userId,
        int id,
        IUserService userService,
        IGameService gameService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var game = await gameService.GetUserGame(userId, id);
        if (game == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("game"));

        await gameService.Remove(game);

        return TypedResults.NoContent();
    }
}
