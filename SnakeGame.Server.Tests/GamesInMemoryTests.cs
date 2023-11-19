using SnakeGame.Server.Models;
using UnitTests.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using SnakeGame.Server.Services.DataServices;

public class GamesInMemoryTests
{
    [Fact]
    public async Task GetAllReturnsGamesFromDatabase()
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        GameService gameService = new(context);

        var result = await GamesEndpoints.GetAllGames(gameService);

        Assert.IsType<Ok<List<GameDto>>>(result);

        Assert.NotNull(result.Value);

        Assert.NotEmpty(result.Value);
        // TODO: add more property tests?
        Assert.Collection(
            result.Value,
            game1 =>
            {
                Assert.Equal(1, game1.Id);
                Assert.Equal("Test name 1", game1.Name);
                Assert.Equal(GameMode.Solo, game1.Mode);
            },
            game2 =>
            {
                Assert.Equal(2, game2.Id);
                Assert.Equal("Test name 2", game2.Name);
                Assert.Equal(GameMode.Duel, game2.Mode);
            }
        );
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public async Task GetGameReturnsGameFromDatabase(int userId, int id)
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var mockUserManager = MockUserManager.GetUserManager<User>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(context.Users.FirstOrDefault(u => u.Id == userId));

        UserService userService = new(mockUserManager.Object);
        GameService gameService = new(context);

        var result = await GamesEndpoints.GetUserGame(userId, id, userService, gameService);

        Assert.IsType<Results<Ok<GameDto>, NotFound<CustomError>>>(result);

        var okResult = (Ok<GameDto>)result.Result;

        Assert.NotNull(okResult.Value);

        Assert.Equal(userId, okResult.Value.Id);
    }

    [Fact]
    public async Task GetAllOpenReturnsOpenGamesFromDatabase()
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        GameService gameService = new(context);

        var result = await GamesEndpoints.GetAllOpenGames(gameService);

        Assert.IsType<Ok<List<GameDto>>>(result);

        Assert.NotNull(result.Value);

        Assert.NotEmpty(result.Value);
        Assert.True(result.Value.All(g => g.IsOpen));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetAllUserReturnsUserGamesFromDatabase(int userId)
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var mockUserManager = MockUserManager.GetUserManager<User>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(context.Users.FirstOrDefault(u => u.Id == userId));

        UserService userService = new(mockUserManager.Object);
        GameService gameService = new(context);

        var result = await GamesEndpoints.GetAllUserGames(userId, userService, gameService);

        Assert.IsType<Results<Ok<List<GameDto>>, NotFound<CustomError>>>(result);

        var okResult = (Ok<List<GameDto>>)result.Result;

        Assert.NotNull(okResult.Value);

        Assert.NotEmpty(okResult.Value);
        Assert.True(okResult.Value.All(g => g.Creator.Id == userId));
    }

    [Theory]
    [InlineData("Test name 3", GameMode.Solo, 1, 1)]
    [InlineData("Test name 4", GameMode.Duel, 2, 1)]
    public async Task CreateUserGameCreatesUserGameInDatabase(
        string name,
        GameMode mode,
        int userId,
        int mapId
    )
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var mockUserManager = MockUserManager.GetUserManager<User>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(context.Users.FirstOrDefault(u => u.Id == userId));

        var httpContext = HttpContextHelper.CreateHttpContext(
            userId,
            new List<string> { UserRoles.Basic }
        );
        UserService userService = new(mockUserManager.Object);
        MapService mapService = new(context);
        GameService gameService = new(context);

        CreateGameDto createGameDto = new(name, mode, mapId);
        CreateGameDtoValidator validator = new(context);

        var initialGameCount = context.Games.Count();

        var result = await GamesEndpoints.CreateUserGame(
            createGameDto,
            validator,
            httpContext,
            userService,
            mapService,
            gameService
        );

        Assert.IsType<
            Results<
                CreatedAtRoute<GameDto>,
                UnprocessableEntity<IEnumerable<CustomError>>,
                UnprocessableEntity<CustomError>,
                NotFound<CustomError>
            >
        >(result);

        var createdResult = (CreatedAtRoute<GameDto>)result.Result;

        Assert.NotNull(createdResult);
        Assert.NotNull(createdResult.RouteName);

        var lastGame = context.Games.Last();
        var gameDiff = context.Games.Count() - initialGameCount;

        Assert.Equal(1, gameDiff);
        Assert.Equal(name, lastGame.Name);
        Assert.Equal(mode, lastGame.Mode);
        Assert.Equal(mapId, lastGame.Map.Id);
        Assert.Equal(userId, lastGame.Creator.Id);
    }

    [Theory]
    [InlineData(2, 2, 1)]
    public async Task UpdateUserGameUpdatesUserGameInDatabase(int userId, int id, int playerId)
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var mockUserManager = MockUserManager.GetUserManager<User>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(
                (string arg) => context.Users.FirstOrDefault(u => u.Id == int.Parse(arg))
            );

        var httpContext = HttpContextHelper.CreateHttpContext(
            playerId,
            new List<string> { UserRoles.Basic }
        );
        UserService userService = new(mockUserManager.Object);
        GameService gameService = new(context);

        var initialPlayerCount = context.Games.Single(g => g.Id == id).Players.Count();

        var result = await GamesEndpoints.UpdateUserGame(
            userId,
            id,
            httpContext,
            userService,
            gameService
        );

        Assert.IsType<
            Results<
                Ok<GameDto>,
                UnprocessableEntity<IEnumerable<CustomError>>,
                UnprocessableEntity<CustomError>,
                NotFound<CustomError>
            >
        >(result);

        var okResult = (Ok<GameDto>)result.Result;

        Assert.NotNull(okResult);

        var updatedGame = context.Games.Single(g => g.Id == id);
        var playerDiff = updatedGame.Players.Count() - initialPlayerCount;

        Assert.Equal(1, playerDiff);
        Assert.True(updatedGame.Players.Any(p => p.Id == playerId));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public async Task RemoveUserGameRemovesUserGameInDatabase(int userId, int id)
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var mockUserManager = MockUserManager.GetUserManager<User>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(context.Users.FirstOrDefault(u => u.Id == userId));

        var httpContext = HttpContextHelper.CreateHttpContext(
            userId,
            new List<string> { UserRoles.Basic }
        );
        UserService userService = new(mockUserManager.Object);
        GameService gameService = new(context);

        var initialGameCount = context.Games.Count();

        var result = await GamesEndpoints.RemoveUserGame(id, httpContext, userService, gameService);

        Assert.IsType<Results<NoContent, UnprocessableEntity<CustomError>, NotFound<CustomError>>>(
            result
        );

        var noContentResult = (NoContent)result.Result;

        Assert.NotNull(noContentResult);

        var gameDiff = initialGameCount - context.Games.Count();

        Assert.Equal(1, gameDiff);
        Assert.False(context.Games.Any(g => g.Id == userId));
    }
}
