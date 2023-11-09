using SnakeGame.Server.Models;

public class Participation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TournamentId { get; set; }

    public User User { get; set; }
    public Tournament Tournament { get; set; }
}

public record ParticipationDto(int Id, User User);

public static class ParticipationMapper
{
    public static ParticipationDto ToDto(this Participation participation)
    {
        return new ParticipationDto(participation.Id, participation.User);
    }
}
