namespace snake_game.Models;

public class Obstacle
{
    public int Id { get; set; }
    public Point[] Points { get; set; }
}

public class ObstacleDto
{
    public Point[] Points { get; set; }
}
