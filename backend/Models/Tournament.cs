namespace snake_game.Models;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int CurrentRound { get; set; } = 1;
    public int MaxParticipants { get; set; }

    public List<Round> Rounds { get; set; } = new();
    public List<Participation> Participations { get; set; } = new();
    public User Organizer { get; set; }
}

public class BaseTournamentDto
{
    public string Name { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int CurrentRound { get; set; }
    public int MaxParticipants { get; set; }
}

public class TournamentDto : BaseTournamentDto
{
    public int Id { get; set; }
    public List<RoundDto> Rounds { get; set; }
    public List<UserDto> Participants { get; set; }
    public UserDto Organizer { get; set; }
}

public class CreateTournamentDto : BaseTournamentDto
{
    public List<CreateRoundDto> Rounds { get; set; }
}

public record UpdateTournamentDto(int ParticipantId);

public static class TournamentMapper
{
    public static TournamentDto ToDto(this Tournament tournament)
    {
        return new TournamentDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            StartTime = tournament.StartTime,
            EndTime = tournament.EndTime,
            CurrentRound = tournament.CurrentRound,
            MaxParticipants = tournament.MaxParticipants,
            Rounds = tournament.Rounds.Select(x => x.ToDto()).ToList(),
            Participants = tournament.Participations.Select(x => x.User.ToDto()).ToList(),
            Organizer = tournament.Organizer.ToDto(),
        };
    }

    public static Tournament FromCreateDto(this CreateTournamentDto dto, User organizer)
    {
        return new Tournament
        {
            Name = dto.Name,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            MaxParticipants = dto.MaxParticipants,
            Rounds = dto.Rounds.Select(x => x.FromCreateDto()).ToList(),
            Organizer = organizer,
        };
    }
}
