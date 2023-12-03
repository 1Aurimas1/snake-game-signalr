using SnakeGame.Server.Services.GameService;

public interface IGameClient
{
    Task ReceiveSnake(List<Vector2> parts);
    Task ReceiveFood(Vector2 position);
    Task ReceiveGameStates(List<GameDto> gameStates, bool initial);
    Task ReceiveCountdown(int i);
}


