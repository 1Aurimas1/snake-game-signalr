using FluentValidation;
using snake_game.Models;

public static class MapsEndpoints
{
    public static RouteGroupBuilder MapMapsApi(this RouteGroupBuilder group)
    {
        group.MapGet("/maps", GetMany);
        group.MapGet("/maps/{id}", Get);
        group.MapPost("/users/{userId}/maps", Create);
        group.MapPatch("/maps/{id}", Update);
        group.MapDelete("/maps/{id}", Remove);

        return group;
    }

    private static IQueryable<Map> GetCompleteQuery(DataContext dbContext)
    {
        return dbContext.Maps
            .Include(x => x.Creator)
            .Include(x => x.MapObstacles)
            .ThenInclude(x => x.Position)
            .AsQueryable();
    }

    public static async Task<IResult> GetMany(DataContext dbContext)
    {
        var maps = await GetCompleteQuery(dbContext).ToListAsync();

        return Results.Ok(maps.Select(x => x.ToDto()));
    }

    public static async Task<IResult> Get(int id, DataContext dbContext)
    {
        var map = await GetCompleteQuery(dbContext).SingleOrDefaultAsync(x => x.Id == id);

        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        return Results.Ok(map.ToDto());
    }

    public static async Task<IResult> Create(int userId, CreateMapDto dto, IValidator<CreateMapDto> validator, DataContext dbContext)
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));


        var uniqueObstacleIds = dto.MapObstacles
            .Select(x => x.ObstacleId)
            .Distinct()
            .ToList();
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

    public static async Task<IResult> Update(
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

        // TODO: different queries?
        var map = await GetCompleteQuery(dbContext)
            .Include(x => x.MapRatings)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        // TODO: unique userId and mapId in MapRating table
        var mapRating = map.MapRatings.SingleOrDefault(x => x.UserId == dto.UserId);
        if (mapRating == null)
        {
            mapRating = new MapRating
            {
                Rating = dto.MapRating,
                UserId = dto.UserId,
            };
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

    //[HttpDelete("{mapId}")]
    public static async Task<IResult> Remove(int id, DataContext dbContext)
    {
        var map = await dbContext.Maps.SingleOrDefaultAsync(x => x.Id == id);
        if (map == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        dbContext.Maps.Remove(map);
        await dbContext.SaveChangesAsync();

        return Results.NoContent();
    }
}
