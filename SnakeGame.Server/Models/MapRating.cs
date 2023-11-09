using SnakeGame.Server.Models;

public class MapRating
{
    public int Id { get; set; }
    public int Rating { get; set; } = 0;
    public int MapId { get; set; }
    public int UserId { get; set; }

    public Map Map { get; set; }
    public User User { get; set; }
}

public record MapRatingDto(int Id, int Rating, int MapId, int UserId);
