public class Vector2
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public Vector2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vector2(Vector2 v)
    {
        X = v.X;
        Y = v.Y;
    }

    public void Plus(Vector2 v)
    {
        X += v.X;
        Y += v.Y;
    }

    public void Assign(Vector2 v)
    {
        X = v.X;
        Y = v.Y;
    }

    public int Dot(Vector2 v)
    {
        return X * v.X + Y * v.Y;
    }

    public static Vector2 operator +(Vector2 v1, Vector2 v2)
    {
        return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
    }

    public static bool operator ==(Vector2 v1, Vector2 v2)
    {
        return v1.X == v2.X && v1.Y == v2.Y;
    }

    public static bool operator !=(Vector2 v1, Vector2 v2)
    {
        return !(v1 == v2);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is Vector2)) return false;

        Vector2 otherPoint = (Vector2)obj;
        return X == otherPoint.X && Y == otherPoint.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}


