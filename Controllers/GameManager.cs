using Microsoft.AspNetCore.SignalR;

public class GameManager
{
    private readonly IHubContext<GameHub, IGameClient> _hubContext;
    private static List<GameRoom> _rooms = new List<GameRoom>();

    private Timer _timer;

    static GameManager() { }

    public GameManager(IHubContext<GameHub, IGameClient> hubContext)
    {
        _hubContext = hubContext;
        _timer = new Timer(RunGameRoomLoop, null, 0, 1000);
    }

    private void RunGameRoomLoop(object? state)
    {
        foreach (GameRoom room in _rooms)
        {
            if (!room.IsStarted && room.IsFull)
            {
                room.Start();
            }
            else if (!room.IsFinished)
            {
                room.UpdateGameStates();
                _hubContext.Clients.Group(room.Id).ReceiveStateObjects(room.GetGameStates());
            }
        }
    }

    public string JoinGameRoom(string playerId)
    {
        var room = _rooms.Find(g => !g.IsFull);
        if (room == null)
        {
            room = new GameRoom();
            _rooms.Add(room);
        }

        room.Games.Add(new Game(playerId, "solo"));
        _hubContext.Groups.AddToGroupAsync(playerId, room.Id);
        return room.Id;
    }

    public void UpdatePlayerMovePosition(string playerId, Direction direction)
    {
        foreach (GameRoom room in _rooms)
        {
            foreach (Game game in room.Games)
            {
                if (game.Player.Id.Equals(playerId))
                {
                    game.Snake.UpdateMovePosition(direction);
                    return;
                }
            }
        }
    }
}


