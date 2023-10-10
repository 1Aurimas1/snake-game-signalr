namespace snake_game.Models;

public class MapObstacle
{
    public int Id { get; set; }
    public Point Position { get; set; }
    public int MapId { get; set; }
    public int ObstacleId { get; set; }
}

public class MapObstacleDto
{
}
