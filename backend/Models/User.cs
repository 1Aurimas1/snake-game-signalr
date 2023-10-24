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

public class BaseUserDto
{
    public string Username { get; set; }
    public string Email { get; set; }
}

public class RegisterUserDto : BaseUserDto
{
    public string Password { get; set; }
    public string PasswordConfirmation { get; set; }
}

public class UpdateUserDto : BaseUserDto { }

public static class UserMapper
{
    public static UserDto ToDto(this User user) => new UserDto(user.Id, user.Username);

    public static User FromRegisterDto(this RegisterUserDto dto)
    {
        return new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = dto.Password,
            UserType = UserType.Basic,
        };
    }

    public static void UpdateWithDto(this User user, UpdateUserDto dto)
    {
        user.Username = dto.Username;
        user.Email = dto.Email;
    }
}
