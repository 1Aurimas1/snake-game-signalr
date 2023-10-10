namespace snake_game.Models;

public class Map
{
    public int Id { get; set; }
    public List<Obstacle> Obstacles { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
}

public class MapDto
{
}
