using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace snake_game.Services.GameService;

[Authorize]
public class GameHub : Hub<IGameClient>
{
    private readonly GameManager _gameManager;

    public GameHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public async Task<string> JoinGame(string playerName, GameMode gameMode)
    {
        return await _gameManager.JoinGameRoom(Context.ConnectionId, playerName, gameMode);
    }

    public async Task<Tuple<int, int>> InitGrid()
    {
        return await Task.Run(() => new Tuple<int, int>(Grid.Rows, Grid.Cols));
    }

    public async Task SendInput(string gameRoomId, string playerName, Direction direction)
    {
        await Task.Run(
            () => _gameManager.UpdatePlayerMovePosition(gameRoomId, playerName, direction)
        );
    }
}
