namespace SnakeGame.Server.Models;

public class Round
{
    public int Id { get; set; }
    public int Index { get; set; }
    public int MapId { get; set; }
    public int TournamentId { get; set; }

    public Map Map { get; set; }
    public Tournament Tournament { get; set; }
}

public record RoundDto(int Id, int Index, int MapId);

public record CreateRoundDto(int Index, int MapId);

public static class RoundMapper
{
    public static RoundDto ToDto(this Round round)
    {
        return new RoundDto(round.Id, round.Index, round.MapId);
    }

    public static Round FromCreateDto(this CreateRoundDto dto)
    {
        return new Round { Index = dto.Index, MapId = dto.MapId };
    }
}
