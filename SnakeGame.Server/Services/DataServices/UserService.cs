using Microsoft.AspNetCore.Identity;
using SnakeGame.Server.Models;

namespace SnakeGame.Server.Services.DataServices;

public interface IUserService
{
    Task<User?> Get(int id);
    Task<List<User>> GetAll();
    Task<IdentityResult> Update(User user, UpdateUserDto dto);
    Task<IdentityResult> Remove(User user);
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
}
