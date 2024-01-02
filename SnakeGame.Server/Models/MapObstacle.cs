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

public record MapObstacleDto(int MapObstacleId, PointDto Position, int ObstacleId);

public record CreateMapObstacleDto(int ObstacleId, CreatePointDto Position);

public static class MapObstacleMapper
{
    public static MapObstacleDto ToDto(this MapObstacle mapObstacle) =>
        new MapObstacleDto(
            mapObstacle.Id,
            mapObstacle.Position.ToDto(),
            mapObstacle.ObstacleId
        );

    public static MapObstacle FromCreateDto(this CreateMapObstacleDto dto)
    {
        return new MapObstacle
        {
            ObstacleId = dto.ObstacleId,
            Position = dto.Position.FromCreateDto()
        };
    }
}
