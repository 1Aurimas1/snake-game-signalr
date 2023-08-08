using Microsoft.AspNetCore.SignalR;

public class GameHub : Hub<IGameClient>
{
    private readonly GameManager _gameManager;

    public GameHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public async Task<string> JoinGame(string playerId)
    {
        playerId = Context.ConnectionId;
        return _gameManager.JoinGameRoom(playerId);
    }

    public async Task<Tuple<int, int>> InitGrid()
    {
        return new Tuple<int, int>(Grid.Rows, Grid.Cols);
    }

    public async Task SendInput(string playerId, Direction direction)
    {
        playerId = Context.ConnectionId;
        _gameManager.UpdatePlayerMovePosition(playerId, direction);
    }
}

