public class Point
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}

public record PointDto(int Id, int X, int Y);

public record CreatePointDto(int X, int Y);

public static class PointMapper
{
    public static Point FromCreateDto(this CreatePointDto dto)
    {
        return new Point { X = dto.X, Y = dto.Y };
    }
}
