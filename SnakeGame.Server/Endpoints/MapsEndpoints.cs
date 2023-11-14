using FluentValidation;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;

public static class MapsEndpoints
{
    public static RouteGroupBuilder MapMapsApi(this RouteGroupBuilder group)
    {
        group.MapGet("/maps", GetAllMaps);
        group.MapPatch("/maps/{id}", PublishMap);
        group.MapGet("/users/{userId}/maps", GetAllUserMaps);
        group.MapGet("/users/{userId}/maps/{id}", GetUserMap);
        group.MapGet("/users/{userId}/tournaments/{tournamentId}/maps", GetAllUserTournamentMaps);
        group.MapPost("/users/{userId}/maps", CreateUserMap);
        group.MapPatch("/users/{userId}/maps/{id}", UpdateUserMap);
        group.MapDelete("/users/{userId}/maps/{id}", RemoveUserMap);

        return group;
    }

    public static async Task<IResult> GetAllMaps(IMapService mapService, bool isPublished = true)
    {
        // TODO: forbidden for basic users
        List<Map> maps = await mapService.GetAllFiltered(x => x.IsPublished == isPublished);

        return Results.Ok(maps.Select(x => x.ToDto()));
    }

    // TODO: forbidden for basic users
    public static async Task<IResult> PublishMap(int id, IMapService mapService)
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

    public static async Task<IResult> CreateUserMap(
        int userId,
        CreateMapDto dto,
        IValidator<CreateMapDto> validator,
        IUserService userService,
        IMapService mapService
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await mapService.Add(dto, user);

        return Results.Created($"/maps/{map.Id}", map.ToDto());
    }

    public static async Task<IResult> UpdateUserMap(
        int userId,
        int id,
        UpdateMapDto dto,
        IValidator<UpdateMapDto> validator,
        IUserService userService,
        IMapService mapService
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var response = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(response);
        }

        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await mapService.GetUserMap(userId, id);
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        await mapService.Update(map, dto);

        return Results.Ok(map.ToDto());
    }

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
}
