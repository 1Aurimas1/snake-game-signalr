namespace snake_game.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsOpen { get; set; }
    public GameMode Mode { get; set; }

    public Map Map { get; set; }
    public User Creator { get; set; }
    public List<User> Players { get; set; } = new();
}

public record GameDto(
    int Id,
    string Name,
    bool IsOpen,
    GameMode Mode,
    int MapId,
    UserDto Creator,
    List<UserDto> Players
);

public record CreateGameDto(string Name, GameMode Mode, int MapId);

public record UpdateGameDto(int PlayerId);

public static class GameMapper
{
    public static GameDto ToDto(this Game game) =>
        new GameDto(
            game.Id,
            game.Name,
            game.IsOpen,
            game.Mode,
            game.Map.Id,
            game.Creator.ToDto(),
            game.Players.Select(x => x.ToDto()).ToList()
        );

    public static Game FromCreateDto(this CreateGameDto dto, Map map, User creator)
    {
        bool isOpen = dto.Mode switch
        {
            GameMode.Solo => false,
            _ => true,
        };

        return new Game
        {
            Name = dto.Name,
            IsOpen = isOpen,
            Mode = dto.Mode,
            Map = map,
            Creator = creator,
            Players = new List<User> { creator },
        };
    }
}
