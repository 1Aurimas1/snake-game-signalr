namespace snake_game.Models;

public class Obstacle
{
    public int Id { get; set; }
    public List<Point> Points { get; set; }
}

public record ObstacleDto(int Id, List<Point> Points);
