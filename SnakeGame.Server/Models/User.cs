using Microsoft.AspNetCore.Identity;

namespace SnakeGame.Server.Models;

public static class UserRoles
{
    public const string Admin = nameof(Admin);
    public const string Basic = nameof(Basic);

    public static readonly IReadOnlyCollection<string> All = new[] { Admin, Basic };
}

public class User : IdentityUser<int>
{
    public List<Participation> Participations { get; set; } = new();

    public bool ForceRelogin { get; set; }
}

public record SuccessfulLoginDto(string AccessToken, string RefreshToken);
public record RefreshAccessTokenDto(string RefreshToken);

public record UserDto(int Id, string UserName);
public record LoginUserDto(string UserName, string Password);

public class BaseUserDto
{
    public string UserName { get; set; }
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
    public static UserDto ToDto(this User user) => new UserDto(user.Id, user.UserName);

    public static User FromRegisterDto(this RegisterUserDto dto)
    {
        return new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
        };
    }
}
