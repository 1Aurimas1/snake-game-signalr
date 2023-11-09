using Microsoft.EntityFrameworkCore;
using SnakeGame.Server.Data;

namespace UnitTests.Helpers;

public class MockDb : IDbContextFactory<DataContext>
{
    public DataContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase($"InMemoryTestDb-{DateTime.Now.ToFileTimeUtc()}")
            .Options;

        return new DataContext(options);
    }
}
