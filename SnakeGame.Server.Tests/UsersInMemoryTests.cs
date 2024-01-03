using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;
using SnakeGame.Tests.Helpers;

public class UsersInMemoryTests
{
    [Fact]
    public async Task GetAllReturnsUsersFromDatabase()
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var mockUserManager = MockUserManager.GetUserManager<User>();
        mockUserManager.Setup(m => m.Users).Returns(context.Users);

        UserService userService = new(mockUserManager.Object);
        GameService gameService = new(context);

        var result = await UsersEndpoints.GetAllUsers(userService);

        Assert.IsType<Ok<List<PrivateUserDto>>>(result);

        Assert.NotNull(result.Value);

        Assert.NotEmpty(result.Value);
        Assert.Collection(
            result.Value,
            user1 =>
            {
                Assert.Equal(1, user1.Id);
                Assert.Equal("Test userName 1", user1.UserName);
            },
            user2 =>
            {
                Assert.Equal(2, user2.Id);
                Assert.Equal("Test userName 2", user2.UserName);
            }
        );
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetUserReturnsUserFromDatabase(int id)
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var mockUserManager = MockUserManager.GetUserManager<User>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(id.ToString()))
            .ReturnsAsync(context.Users.FirstOrDefault(u => u.Id == id));

        var httpContext = HttpContextHelper.CreateHttpContext(
            id,
            new List<string> { UserRoles.Basic }
        );
        UserService userService = new(mockUserManager.Object);

        var result = await UsersEndpoints.GetUser(id, httpContext, userService);

        Assert.IsType<Results<Ok<PrivateUserDto>, ForbidHttpResult, NotFound<CustomError>>>(result);

        var okResult = (Ok<PrivateUserDto>)result.Result;

        Assert.NotNull(okResult.Value);

        Assert.Equal(id, okResult.Value.Id);
    }

    [Theory]
    [InlineData(1, "Test userName 1337", "email1337@test.com")]
    [InlineData(2, "Test userName 1338", "email1338@test.com")]
    public async Task UpdateUserUpdatesUserInDatabase(int id, string newName, string newEmail)
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var mockUserManager = MockUserManager.GetUserManager<User>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(id.ToString()))
            .ReturnsAsync(context.Users.FirstOrDefault(u => u.Id == id));
        mockUserManager
            .Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var httpContext = HttpContextHelper.CreateHttpContext(
            id,
            new List<string> { UserRoles.Basic }
        );
        UserService userService = new(mockUserManager.Object);

        var updateUserDto = new UpdateUserDto { UserName = newName, Email = newEmail };

        var result = await UsersEndpoints.UpdateUser(id, httpContext, updateUserDto, userService);

        Assert.IsType<
            Results<
                Ok<PrivateUserDto>,
                UnprocessableEntity<CustomError>,
                ForbidHttpResult,
                NotFound<CustomError>
            >
        >(result);

        var okResult = (Ok<PrivateUserDto>)result.Result;

        Assert.NotNull(okResult);

        var updatedUser = await userService.Get(id);

        Assert.NotNull(updatedUser);

        Assert.Equal(newName, updatedUser.UserName);
        Assert.Equal(newEmail, updatedUser.Email);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task RemoveUserRemovesUserInDatabase(int id)
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var mockUserManager = MockUserManager.GetUserManager<User>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(id.ToString()))
            .ReturnsAsync(context.Users.FirstOrDefault(u => u.Id == id));
        mockUserManager
            .Setup(m => m.DeleteAsync(It.IsAny<User>()))
            .Callback(
                async (User arg) =>
                {
                    context.Users.Remove(arg);
                    await context.SaveChangesAsync();
                }
            )
            .ReturnsAsync(IdentityResult.Success);

        UserService userService = new(mockUserManager.Object);

        var initialUserCount = context.Users.Count();

        var result = await UsersEndpoints.RemoveUser(id, userService);

        Assert.IsType<Results<NoContent, NotFound<CustomError>, UnprocessableEntity<string>>>(
            result
        );

        var noContentResult = (NoContent)result.Result;

        Assert.NotNull(noContentResult);

        var userDiff = initialUserCount - context.Users.Count();

        Assert.Equal(1, userDiff);
        Assert.False(context.Users.Any(g => g.Id == id));
    }
}
