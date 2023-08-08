class Game
{
    private Point _food;

    private bool _toUpdateFood;
    private bool _isStarted;
    private bool _isFinished;

    public Player Player { get; }
    public Snake Snake;
    public bool IsRunning
    {
        get
        {
            return !(_isStarted || _isFinished);
        }
    }


    public Game(string playerId, string playerName)
    {
        Snake = new Snake();
        _food = SpawnFood();

        _toUpdateFood = true;
        _isStarted = false;
        _isFinished = false;

        Player = new Player(playerId, playerName);
    }

    public void UpdateState()
    {
        if (!IsRunning) return;
        ResetUpdateChecks();

        Point nextPos = Snake.NextHeadPosition();
        if (IsColliding(nextPos))
        {
            _isFinished = true;
        }
        else if (_food == nextPos)
        {
            Snake.Grow();
            _food = SpawnFood();
            _toUpdateFood = true;
        }
        else if (Grid.Size == Snake.Length() + 1)
        {
            _isFinished = true;
        }
        else
        {
            Snake.Move();
        }
    }

    private void ResetUpdateChecks()
    {
        _toUpdateFood = false;
    }

    public GameDto GetCurrentState()
    {
        return new GameDto()
        {
            PlayerId = Player.Id,
            SnakeParts = Snake.Parts(),
            Food = _toUpdateFood ? _food : null,
        };
    }

    private bool IsColliding(Point p)
    {
        return !Grid.IsWithinBoundaries(p) || Snake.IsColliding(p);
    }

    private Point SpawnFood()
    {
        Point[] emptyCells = FindEmptyCells();

        Random rnd = new Random();
        int idx = rnd.Next(0, emptyCells.Length);

        return emptyCells[idx];
    }

    private Point[] FindEmptyCells()
    {
        Point[] emptyCells = new Point[Grid.Size - Snake.Length()];

        int idx = 0;
        for (int i = 0; i < Grid.Rows; i++)
        {
            for (int j = 0; j < Grid.Cols; j++)
            {
                Point cell = new Point(i, j);
                if (!Snake.IsColliding(cell))
                {
                    emptyCells[idx++] = cell;
                }
            }
        }

        return emptyCells;
    }
}


