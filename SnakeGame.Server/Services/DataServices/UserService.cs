using Microsoft.AspNetCore.Identity;
using SnakeGame.Server.Models;

namespace SnakeGame.Server.Services.DataServices;

public interface IUserService
{
    Task<IdentityResult> ChangePassword(User user, string oldPassword, string newPassword);
    Task<User?> Get(int id);
    Task<List<User>> GetAll();
    Task<User?> GetByName(string userName);
    Task<(User user, IList<string> roles)?> Login(string userName, string password);
    Task<IEnumerable<IdentityError>?> Register(User user, string password);
    Task<IdentityResult> Remove(User user);
    Task<IdentityResult> Update(User user, UpdateUserDto dto);
    Task UpdateForceRelogin(User user, bool toRelog);
}

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<User>> GetAll()
    {
        return await _userManager.Users.ToListAsync();
    }

    public async Task<User?> Get(int id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<User?> GetByName(string userName)
    {
        return await _userManager.FindByNameAsync(userName);
    }

    public async Task<IdentityResult> Update(User user, UpdateUserDto dto)
    {
        if (!string.IsNullOrEmpty(dto.UserName) && string.IsNullOrEmpty(dto.Email))
        {
            user.UserName = dto.UserName;
        }
        else if (string.IsNullOrEmpty(dto.UserName) && !string.IsNullOrEmpty(dto.Email))
        {
            user.Email = dto.Email;
        }
        else
        {
            user.UserName = dto.UserName;
            user.Email = dto.Email;
        }

        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> Remove(User user)
    {
        return await _userManager.DeleteAsync(user);
    }

    public async Task<IEnumerable<IdentityError>?> Register(User user, string password)
    {
        var userResult = await _userManager.CreateAsync(user, password);
        if (!userResult.Succeeded)
            return userResult.Errors;

        var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Basic);
        if (!roleResult.Succeeded)
            return roleResult.Errors;

        return null;
    }

    public async Task UpdateForceRelogin(User user, bool toRelog)
    {
        user.ForceRelogin = toRelog;
        await _userManager.UpdateAsync(user);
    }

    public async Task<(User user, IList<string> roles)?> Login(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            return null;

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return (user, roles);
    }

    public async Task<IdentityResult> ChangePassword(
        User user,
        string oldPassword,
        string newPassword
    )
    {
        return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
    }
}
