static class MovePositions
{
    private static readonly Dictionary<Direction, Vector2> _positions;

    static MovePositions()
    {
        _positions = new Dictionary<Direction, Vector2>();
        _positions.Add(Direction.Up, new Vector2(0, 1));
        _positions.Add(Direction.Down, new Vector2(0, -1));
        _positions.Add(Direction.Left, new Vector2(-1, 0));
        _positions.Add(Direction.Right, new Vector2(1, 0));
    }

    public static Vector2 Get(Direction direction)
    {
        return _positions[direction];
    }
}


