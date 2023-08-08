class GameRoom
{
    private readonly uint _maxGames;
    private List<GameDto> _gameStates;

    public string Id { get; }
    public List<Game> Games { get; private set; }

    public bool IsFull { get => Games.Count >= _maxGames; }
    public bool IsStarted { get; private set; }
    public bool IsFinished { get; private set; }

    public GameRoom()
    {
        _maxGames = 1;
        _gameStates = new List<GameDto>();

        Id = Guid.NewGuid().ToString();
        Games = new List<Game>();

        IsStarted = false;
        IsFinished = false;
    }

    public void Start()
    {
        IsStarted = true;
    }

    public List<GameDto> GetGameStates()
    {
        return _gameStates;
    }

    public void UpdateGameStates()
    {
        for (int i = 0; i < Games.Count; i++)
        {
            if (Games[i].IsRunning)
            {
                if (_gameStates.Count <= i)
                {
                    _gameStates.Add(Games[i].GetCurrentState());
                }
                else
                {
                    _gameStates[i] = Games[i].GetCurrentState();
                }
                Games[i].UpdateState();
            }
        }
    }
}


