namespace SnakeGame.Server.Models;

public class MapObstacle
{
    public int Id { get; set; }
    public int PositionId { get; set; }
    public int ObstacleId { get; set; }
    public int MapId { get; set; }

    public Point Position { get; set; }
    public Obstacle Obstacle { get; set; }
    public Map Map { get; set; }
}

public record MapObstacleDto(int ObstacleId, Point Position);

public record CreateMapObstacleDto(int ObstacleId, int MapId, CreatePointDto Position);

public static class MapObstacleMapper
{
    public static MapObstacleDto ToDto(this MapObstacle mapObstacle) =>
        new MapObstacleDto(mapObstacle.Id, mapObstacle.Position);

    public static MapObstacle FromCreateDto(this CreateMapObstacleDto dto)
    {
        return new MapObstacle
        {
            ObstacleId = dto.ObstacleId,
            MapId = dto.MapId,
            Position = dto.Position.FromCreateDto()
        };
    }
}
