using Microsoft.EntityFrameworkCore;
using SnakeGame.Server.Data;
using SnakeGame.Server.Models;

namespace SnakeGame.Tests.Helpers;

public class MockDb : IDbContextFactory<DataContext>
{
    public DataContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase($"InMemoryTestDb-{DateTime.Now.ToFileTimeUtc()}")
            .Options;

        return new DataContext(options);
    }

    public static async Task SeedAsync(DataContext context)
    {
        var newUsers = new User[]
        {
            new User
            {
                Id = 1,
                UserName = "Test userName 1",
                Email = "Test email 1",
                PasswordHash = "secretHash1",
            },
            new User
            {
                Id = 2,
                UserName = "Test userName 2",
                Email = "Test email 2",
                PasswordHash = "secretHash2",
            }
        };

        var newMap = new Map
        {
            Id = 1,
            Name = "Test name 1",
            IsPublished = true,
            Creator = newUsers[0],
            MapRatings = new(),
            MapObstacles = new()
        };

        context.Games.AddRange(
            new Game
            {
                Id = 1,
                Name = "Test name 1",
                IsOpen = false,
                Mode = GameMode.Solo,
                Map = newMap,
                Creator = newUsers[0],
            },
            new Game
            {
                Id = 2,
                Name = "Test name 2",
                IsOpen = true,
                Mode = GameMode.Duel,
                Map = newMap,
                Creator = newUsers[1],
            }
        );

        await context.SaveChangesAsync();
    }
}
