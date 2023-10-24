public class GameDto
{
    public string PlayerName { get; set; }
    public List<Vector2> SnakeParts { get; set; }
    public Vector2? Food { get; set; }
    public int? Score { get; set; }
}


