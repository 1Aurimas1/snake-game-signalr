using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SnakeGame.Server.Data;

namespace SnakeGame.Tests.Helpers;

class SnakeGameApplication : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var root = new InMemoryDatabaseRoot();

        builder.ConfigureServices(services =>
        {
            services.AddScoped(sp =>
            {
                var dbContext = new DbContextOptionsBuilder<DataContext>()
                    .UseInMemoryDatabase("Tests", root)
                    .UseApplicationServiceProvider(sp)
                    .Options;

                return dbContext;
            });
        });

        return base.CreateHost(builder);
    }
}
