namespace snake_game.Models;

public class Map
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Rating { get; set; } = 0;
    public bool IsPublished { get; set; }

    public List<MapRating> MapRatings { get; set; }
    public User Creator { get; set; }
    public List<MapObstacle> MapObstacles { get; set; }
}

public record MapDto(
    int Id,
    string Name,
    List<MapObstacleDto> MapObstacles,
    double Rating,
    UserDto Creator
);

public record CreateMapDto(string Name, List<CreateMapObstacleDto> MapObstacles);

public record UpdateMapDto(int MapRating, int UserId);

public static class MapMapper
{
    public static MapDto ToDto(this Map map) =>
        new MapDto(
            map.Id,
            map.Name,
            map.MapObstacles.Select(o => o.ToDto()).ToList(),
            map.Rating,
            map.Creator.ToDto()
        );

    public static Map FromCreateDto(
        this CreateMapDto dto,
        List<MapObstacle> mapObstacles,
        User creator
    )
    {
        return new Map
        {
            Name = dto.Name,
            MapObstacles = mapObstacles,
            IsPublished = false,
            Creator = creator,
        };
    }
}
