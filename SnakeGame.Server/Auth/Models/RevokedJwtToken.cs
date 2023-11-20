using SnakeGame.Server.Models;

namespace SnakeGame.Server.Auth.Models;

public class RevokedJwtToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpirationDate { get; set; }

    public User User { get; set; } = null!;
}
