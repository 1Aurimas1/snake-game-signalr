class Snake
{
    private readonly byte _initSize;
    private List<Vector2> _parts;
    private Vector2 _movePos;

    public int Growth { get => _parts.Count - _initSize; }

    public Snake()
    {
        _initSize = 3;

        _parts = new List<Vector2>();
        for (int i = _initSize - 1; i >= 0; i--)
            _parts.Add(new Vector2(i, 0));

        _movePos = _parts.Last() + MovePositions.Get(Direction.Right);
    }

    public void UpdateMovePosition(Direction direction)
    {
        var newPos = MovePositions.Get(direction);
        if (_movePos.Dot(newPos) >= 0)
        {
            _movePos = newPos;
        }
    }

    public void Move()
    {
        int tailIdx = _parts.Count - 1;

        for (int i = tailIdx; i > 0; i--)
        {
            _parts[i].Assign(_parts[i - 1]);
        }
        _parts[0].Plus(_movePos);
    }

    public void Grow()
    {
        // TODO: consider using linked list?
        _parts.Insert(0, _parts[0] + _movePos);
    }

    public int Length()
    {
        return _parts.Count;
    }

    public List<Vector2> Parts()
    {
        return _parts;
    }

    public Vector2 NextHeadPosition()
    {
        return _parts[0] + _movePos;
    }

    public bool IsColliding(Vector2 p)
    {
        return _parts.Contains(p);
    }
}


