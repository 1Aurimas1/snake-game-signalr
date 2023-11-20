using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using SnakeGame.Server.Filters;
using SnakeGame.Server.Helpers;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;

public static class GamesEndpoints
{
    public static RouteGroupBuilder MapGamesApi(this RouteGroupBuilder group)
    {
        group.MapGet("/games", GetAllGames);
        group.MapGet("/games/open", GetAllOpenGames);
        group.MapGet("/users/{userId}/games", GetAllUserGames);
        group.MapGet("/users/{userId}/games/{id}", GetUserGame).WithName(nameof(GetUserGame));
        group
            .MapPost("/games", CreateUserGame)
            .AddEndpointFilter<ValidationFilter<CreateGameDto>>();
        group.MapPatch("/users/{userId}/games/{id}", UpdateUserGame);
        group.MapDelete("/games/{id}", RemoveUserGame);

        return group;
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<Ok<List<GameDto>>> GetAllGames(IGameService gameService)
    {
        var games = await gameService.GetAll();

        return TypedResults.Ok(games.Select(x => x.ToDto()).ToList());
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<Ok<List<GameDto>>> GetAllOpenGames(IGameService gameService)
    {
        var games = await gameService.GetAllFiltered(g => g.IsOpen);

        return TypedResults.Ok(games.Select(x => x.ToDto()).ToList());
    }

    [Authorize(Roles = UserRoles.Basic)]
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

    [Authorize(Roles = UserRoles.Basic)]
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

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<
        Results<CreatedAtRoute<GameDto>, UnprocessableEntity<CustomError>, NotFound<CustomError>>
    > CreateUserGame(
        CreateGameDto dto,
        HttpContext httpContext,
        IUserService userService,
        IMapService mapService,
        IGameService gameService
    )
    {
        if (!httpContext.TryGetJwtUserId(out int userId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "UserId",
                    "Invalid user ID"
                )
            );

        var user = await userService.Get(userId);
        if (user == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await mapService.Get(dto.MapId);
        if (map == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        var game = dto.FromCreateDto(map, user);
        await gameService.Add(game);

        return TypedResults.CreatedAtRoute(
            game.ToDto(),
            nameof(GetUserGame),
            new { userId = userId, id = game.Id }
        );
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<
        Results<Ok<GameDto>, UnprocessableEntity<CustomError>, NotFound<CustomError>>
    > UpdateUserGame(
        int userId,
        int id,
        HttpContext httpContext,
        IUserService userService,
        IGameService gameService
    )
    {
        if (!httpContext.TryGetJwtUserId(out int playerId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "UserId",
                    "Invalid user ID"
                )
            );

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

        var player = await userService.Get(playerId);
        if (player == null)
            return TypedResults.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("player"));
        else if (game.Players.Exists(x => x.Id == playerId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "player",
                    "Cannot join same game more than once"
                )
            );

        await gameService.Update(game, player);

        return TypedResults.Ok(game.ToDto());
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<
        Results<NoContent, UnprocessableEntity<CustomError>, NotFound<CustomError>>
    > RemoveUserGame(
        int id,
        HttpContext httpContext,
        IUserService userService,
        IGameService gameService
    )
    {
        if (!httpContext.TryGetJwtUserId(out int userId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "UserId",
                    "Invalid user ID"
                )
            );

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
