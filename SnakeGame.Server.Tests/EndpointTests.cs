using SnakeGame.Server.Models;
using UnitTests.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

public class EndpointTests
{
    [Fact]
    public async Task GetGameReturnsGameFromDatabase()
    {
        await using var context = new MockDb().CreateDbContext();

        var newUser = new User
        {
            Id = 1,
            UserName = "Test userName",
            Email = "Test email",
            PasswordHash = "secretHash",
        };

        var newMap = new Map
        {
            Id = 1,
            Name = "Test name",
            IsPublished = true,
            Creator = newUser,
            MapRatings = new(),
            MapObstacles = new()
        };

        context.Games.Add(
            new Game
            {
                Id = 1,
                Name = "Test name",
                IsOpen = false,
                Mode = GameMode.Solo,
                Map = newMap,
                Creator = newUser,
            }
        );
        await context.SaveChangesAsync();

        var result = await GamesEndpoints.Get(1, 1, context);

        Assert.IsType<Ok<GameDto>>(result);

        var okResult = (Ok<GameDto>)result;

        Assert.NotNull(okResult.Value);
        Assert.Equal(1, okResult.Value.Id);
    }
}
