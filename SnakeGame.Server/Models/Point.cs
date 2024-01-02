using System.Text.Json.Serialization;

namespace SnakeGame.Server.Models;

public class Point
{
    public int Id { get; set; }

    [JsonPropertyName("X")]
    public int X { get; set; }

    [JsonPropertyName("Y")]
    public int Y { get; set; }
}

public record PointDto(int Id)
{
    [JsonPropertyName("X")]
    public int X { get; init; }

    [JsonPropertyName("Y")]
    public int Y { get; init; }
}

public record CreatePointDto(int X, int Y);

public static class PointMapper
{
    public static PointDto ToDto(this Point point) =>
        new PointDto(point.Id) { X = point.X, Y = point.Y };

    public static Point FromCreateDto(this CreatePointDto dto)
    {
        return new Point { X = dto.X, Y = dto.Y };
    }
}
