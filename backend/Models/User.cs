namespace snake_game.Models;

public enum UserType
{
    Basic,
    Admin,
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public UserType UserType { get; set; }

    public List<Participation> Participations { get; set; } = new();
}

public record UserDto(int Id, string Username);

public record LoginUserDto(string Username, string Password);

public class BaseUserDto
{
    public string Username { get; set; }
    public string Email { get; set; }
}

public class CreateUserDto : BaseUserDto
{
    public string Password { get; set; }
    public string PasswordConfirmation { get; set; }
}

public class UpdateUserDto : BaseUserDto { }

public static class UserMapper
{
    public static UserDto ToDto(this User user) => new UserDto(user.Id, user.Username);

    public static User FromCreateDto(this CreateUserDto dto, string passwordHash)
    {
        return new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = passwordHash,
            UserType = UserType.Basic,
        };
    }

    public static void UpdateWithDto(this User user, UpdateUserDto dto)
    {
        if (!string.IsNullOrEmpty(dto.Username) && string.IsNullOrEmpty(dto.Email))
        {
            user.Username = dto.Username;
        }
        else if (string.IsNullOrEmpty(dto.Username) && !string.IsNullOrEmpty(dto.Email))
        {
            user.Email = dto.Email;
        }
        else
        {
            user.Username = dto.Username;
            user.Email = dto.Email;
        }
    }
}
