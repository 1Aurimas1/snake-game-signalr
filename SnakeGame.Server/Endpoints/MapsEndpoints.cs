using FluentValidation;
using SnakeGame.Server.Models;

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

    private static IQueryable<Map> WithIncludes(this IQueryable<Map> query)
    {
        return query
            .Include(x => x.Creator)
            .Include(x => x.MapObstacles)
            .ThenInclude(x => x.Position);
    }

    public static async Task<IResult> GetAllMaps(DataContext dbContext, bool isPublished = true)
    {
        List<Map> maps;
        if (isPublished)
        {
            maps = await dbContext.Maps.WithIncludes().Where(x => x.IsPublished).ToListAsync();
        }
        else
        {
            // TODO: forbidden for basic users
            maps = await dbContext.Maps.WithIncludes().Where(x => !x.IsPublished).ToListAsync();
        }

        return Results.Ok(maps.Select(x => x.ToDto()));
    }

    // TODO: forbidden for basic users
    public static async Task<IResult> PublishMap(int id, DataContext dbContext)
    {
        var map = await dbContext.Maps.WithIncludes().FirstOrDefaultAsync(x => x.Id == id);
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

        map.IsPublished = true;

        await dbContext.SaveChangesAsync();

        return Results.Ok(map.ToDto());
    }

    public static async Task<IResult> GetAllUserMaps(int userId, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var maps = await dbContext.Maps
            .WithIncludes()
            .Where(x => x.Creator.Id == userId)
            .ToListAsync();

        return Results.Ok(maps.Select(x => x.ToDto()));
    }

    public static async Task<IResult> GetUserMap(int userId, int id, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await dbContext.Maps
            .WithIncludes()
            .FirstOrDefaultAsync(x => x.Id == id && x.Creator.Id == userId);
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        return Results.Ok(map.ToDto());
    }

    public static async Task<IResult> GetAllUserTournamentMaps(
        int userId,
        int tournamentId,
        DataContext dbContext
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await dbContext.Tournaments
            .Include(x => x.Organizer)
            .FirstOrDefaultAsync(x => x.Id == tournamentId && x.Organizer.Id == userId);
        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        var rounds = await dbContext.Rounds
            .Include(x => x.Map)
            .ThenInclude(x => x.MapRatings)
            .Include(x => x.Map)
            .ThenInclude(x => x.MapObstacles)
            .Include(x => x.Map)
            .ThenInclude(x => x.Creator)
            .Where(x => x.TournamentId == tournamentId)
            .ToListAsync();

        var mapsDtos = rounds.Select(x => x.Map.ToDto());

        return Results.Ok(mapsDtos);
    }

    public static async Task<IResult> CreateUserMap(
        int userId,
        CreateMapDto dto,
        IValidator<CreateMapDto> validator,
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

        var uniqueObstacleIds = dto.MapObstacles.Select(x => x.ObstacleId).Distinct().ToList();
        var uniqueObstacles = dbContext.Obstacles
            .Where(x => uniqueObstacleIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x);

        List<MapObstacle> mapObstacles = new();
        foreach (var mapObstacleDto in dto.MapObstacles)
            if (uniqueObstacles.ContainsKey(mapObstacleDto.ObstacleId))
                mapObstacles.Add(mapObstacleDto.FromCreateDto());

        var map = dto.FromCreateDto(mapObstacles, user);
        dbContext.Maps.Add(map);

        await dbContext.SaveChangesAsync();

        return Results.Created($"/maps/{map.Id}", map.ToDto());
    }

    public static async Task<IResult> UpdateUserMap(
        int userId,
        int id,
        UpdateMapDto dto,
        IValidator<UpdateMapDto> validator,
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

        // TODO: different queries?
        var map = await dbContext.Maps
            .WithIncludes()
            .Include(x => x.MapRatings)
            .FirstOrDefaultAsync(x => x.Id == id && x.Creator.Id == userId);
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        // TODO: unique userId and mapId in MapRating table
        var mapRating = map.MapRatings.FirstOrDefault(
            x => x.UserId == dto.UserId && x.MapId == map.Id
        );
        if (mapRating == null)
        {
            mapRating = new MapRating { Rating = dto.MapRating, UserId = dto.UserId, };
            map.MapRatings.Add(mapRating);
        }
        else
        {
            mapRating.Rating = dto.MapRating;
        }

        var newRating = (double)map.MapRatings.Sum(x => x.Rating) / map.MapRatings.Count;

        // TODO?: if user gets deleted update rating
        map.Rating = newRating;

        await dbContext.SaveChangesAsync();

        return Results.Ok(map.ToDto());
    }

    public static async Task<IResult> RemoveUserMap(int userId, int id, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var map = await dbContext.Maps.FirstOrDefaultAsync(
            x => x.Id == id && x.Creator.Id == userId
        );
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        dbContext.Maps.Remove(map);
        await dbContext.SaveChangesAsync();

        return Results.NoContent();
    }
}
