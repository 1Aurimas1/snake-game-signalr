namespace SnakeGame.Server.Models;

public class Obstacle
{
    public int Id { get; set; }
    public List<Point> Points { get; set; }
}

public record ObstacleDto(int Id, List<PointDto> Points);

public static class ObstacleMapper
{
    public static ObstacleDto ToDto(this Obstacle obstacle) =>
        new ObstacleDto(obstacle.Id, obstacle.Points.Select(p => p.ToDto()).ToList());
}
