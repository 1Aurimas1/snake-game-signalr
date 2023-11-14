using System.Linq.Expressions;
using SnakeGame.Server.Models;

namespace SnakeGame.Server.Services.DataServices;

public interface IMapService
{
    Task<List<Map>> GetAll();
    Task<List<Map>> GetAllFiltered(Expression<Func<Map, bool>> predicate);
    Task<Map?> Get(int id);
    Task<Map?> GetUserMap(int userId, int id);
    Task<Map> Add(CreateMapDto dto, User creator);
    Task Update(Map map, UpdateMapDto dto);
    Task Remove(Map map);
    Task Publish(Map map);
}

public class MapService : IMapService
{
    private readonly DataContext _dbContext;

    public MapService(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Map>> GetAll()
    {
        return await _dbContext.Maps.WithIncludes().ToListAsync();
    }

    public async Task<List<Map>> GetAllFiltered(Expression<Func<Map, bool>> predicate)
    {
        return await _dbContext.Maps.WithIncludes().Where(predicate).ToListAsync();
    }

    public async Task<Map?> Get(int id)
    {
        return await _dbContext.Maps.WithIncludes().FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Map?> GetUserMap(int userId, int id)
    {
        return await _dbContext.Maps
            .WithIncludes()
            .FirstOrDefaultAsync(m => m.Creator.Id == userId && m.Id == id);
    }

    public async Task<Map> Add(CreateMapDto dto, User creator)
    {
        var uniqueObstacleIds = dto.MapObstacles.Select(x => x.ObstacleId).Distinct().ToList();
        var uniqueObstacles = _dbContext.Obstacles
            .Where(x => uniqueObstacleIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x);

        List<MapObstacle> mapObstacles = new();
        foreach (var mapObstacleDto in dto.MapObstacles)
            if (uniqueObstacles.ContainsKey(mapObstacleDto.ObstacleId))
                mapObstacles.Add(mapObstacleDto.FromCreateDto());

        var map = dto.FromCreateDto(mapObstacles, creator);

        await _dbContext.Maps.AddAsync(map);

        await _dbContext.SaveChangesAsync();

        return map;
    }

    public async Task Update(Map map, UpdateMapDto dto)
    {
        // TODO: unique userId and mapId in MapRating table
        var mapRating = map.MapRatings.FirstOrDefault(
            x => x.MapId == map.Id && x.UserId == dto.UserId
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

        await _dbContext.SaveChangesAsync();
    }

    public async Task Remove(Map map)
    {
        _dbContext.Maps.Remove(map);

        await _dbContext.SaveChangesAsync();
    }

    public async Task Publish(Map map)
    {
        map.IsPublished = true;

        await _dbContext.SaveChangesAsync();
    }
}

public static class MapQueryExtensions
{
    public static IQueryable<Map> WithIncludes(this IQueryable<Map> query)
    {
        return query
            .Include(x => x.Creator)
            .Include(x => x.MapObstacles)
            .ThenInclude(x => x.Position)
            .Include(x => x.MapRatings);
    }
}
