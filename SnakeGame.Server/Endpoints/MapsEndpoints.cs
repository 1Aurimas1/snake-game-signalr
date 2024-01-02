using System.Linq.Expressions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using SnakeGame.Server.Filters;
using SnakeGame.Server.Helpers;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;

public static class MapsEndpoints
{
    public static RouteGroupBuilder MapMapsApi(this RouteGroupBuilder group)
    {
        // crud
        group.MapGet("/maps", GetAllMaps);
        group.MapGet("/users/{userId}/maps/{id}", GetUserMap).WithName(nameof(GetUserMap));
        group.MapPost("/maps", CreateUserMap).AddEndpointFilter<ValidationFilter<CreateMapDto>>();
        group
            .MapPatch("/users/{userId}/maps/{id}", UpdateUserMap)
            .AddEndpointFilter<ValidationFilter<UpdateMapDto>>();
        group.MapDelete("/users/{userId}/maps/{id}", RemoveUserMap);

        // additional
        group.MapGet("/users/{userId}/maps", GetAllUserMaps);
        group.MapGet("/users/{userId}/tournaments/{tournamentId}/maps", GetAllUserTournamentMaps);
        group.MapPatch("/users/{userId}/maps/{id}/status", PublishMap);
        group.MapGet("/obstacles", GetAllObstacles);
        group.MapGet("/obstacles/{ids}", GetAllObstaclesByIds);

        return group;
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> GetAllMaps(
        HttpContext httpContext,
        IMapService mapService,
        bool? isPublished
    )
    {
        Expression<Func<Map, bool>> predicate;
        if (httpContext.User.IsInRole(UserRoles.Admin))
            predicate = isPublished == null ? x => true : x => x.IsPublished == isPublished;
        else if (httpContext.User.IsInRole(UserRoles.Basic))
            predicate = x => x.IsPublished;
        else
            return Results.Forbid();

        List<Map> maps = await mapService.GetAllFiltered(predicate);

        return Results.Ok(maps.Select(x => x.ToDto()));
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> GetUserMap(
        int userId,
        int id,
        IUserService userService,
        IMapService mapService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await mapService.GetUserMap(userId, id);
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        return Results.Ok(map.ToDto());
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> CreateUserMap(
        HttpContext httpContext,
        CreateMapDto dto,
        IUserService userService,
        IMapService mapService
    )
    {
        if (!httpContext.TryGetJwtUserId(out int userId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "userId",
                    "Invalid user ID"
                )
            );

        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await mapService.Add(dto, user);

        return TypedResults.CreatedAtRoute(
            map.ToDto(),
            nameof(GetUserMap),
            new { userId = userId, id = map.Id }
        );
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> UpdateUserMap(
        int userId,
        int id,
        HttpContext httpContext,
        UpdateMapDto dto,
        IUserService userService,
        IMapService mapService
    )
    {
        if (!httpContext.TryGetJwtUserId(out int ratingUserId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "userId",
                    "Invalid user ID"
                )
            );

        var resourceCreator = await userService.Get(userId);
        if (resourceCreator == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var user = await userService.Get(ratingUserId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("rating user"));

        var map = await mapService.GetUserMap(userId, id);
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        await mapService.Update(map, dto, ratingUserId);

        return Results.Ok(map.ToDto());
    }

    [Authorize(Roles = UserRoles.Admin)]
    public static async Task<IResult> RemoveUserMap(
        int userId,
        int id,
        IUserService userService,
        IMapService mapService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await mapService.GetUserMap(userId, id);
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        await mapService.Remove(map);

        return Results.NoContent();
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> GetAllUserMaps(
        int userId,
        IUserService userService,
        IMapService mapService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var maps = await mapService.GetAllFiltered(x => x.Creator.Id == userId);

        return Results.Ok(maps.Select(x => x.ToDto()));
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> GetAllUserTournamentMaps(
        int userId,
        int tournamentId,
        IUserService userService,
        ITournamentService tournamentService,
        IMapService mapService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await tournamentService.GetUserTournament(userId, tournamentId);
        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        var rounds = await tournamentService.GetAllTournamentRounds(tournamentId);
        var mapsDtos = rounds.Select(x => x.Map.ToDto());

        return Results.Ok(mapsDtos);
    }

    [Authorize(Roles = UserRoles.Admin)]
    public static async Task<IResult> PublishMap(int userId, int id, IMapService mapService)
    {
        var map = await mapService.Get(id);
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        if (map.IsPublished)
        {
            var responseResult = JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                "mapId",
                "Map is already published"
            );
            return Results.UnprocessableEntity(responseResult);
        }

        await mapService.Publish(map);

        return Results.Ok(map.ToDto());
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> GetAllObstacles(
        HttpContext httpContext,
        IMapService mapService
    )
    {
        var obstacles = await mapService.GetAllObstacles();

        return Results.Ok(obstacles.Select(x => x.ToDto()));
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> GetAllObstaclesByIds(
        int[] ids,
        HttpContext httpContext,
        IMapService mapService
    )
    {
        var obstacles = await mapService.GetAllObstaclesByIds(ids);

        return Results.Ok(obstacles.Select(x => x.ToDto()));
    }
}
