using SnakeGame.Server.Models;
using UnitTests.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

public class GamesInMemoryTests
{
    [Fact]
    public async Task GetAllReturnsGamesFromDatabase()
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var result = await GamesEndpoints.GetAllGames(context);

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

    [Fact]
    public async Task GetGameReturnsGameFromDatabase()
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var result = await GamesEndpoints.GetUserGame(1, 1, context);

        Assert.IsType<Results<Ok<GameDto>, NotFound<CustomError>>>(result);

        var okResult = (Ok<GameDto>)result.Result;

        Assert.NotNull(okResult.Value);

        Assert.Equal(1, okResult.Value.Id);
    }

    [Fact]
    public async Task GetAllOpenReturnsOpenGamesFromDatabase()
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);

        var result = await GamesEndpoints.GetAllOpenGames(context);

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

        var result = await GamesEndpoints.GetAllUserGames(userId, context);

        Assert.IsType<Results<Ok<List<GameDto>>, NotFound<CustomError>>>(result);

        var okResult = (Ok<List<GameDto>>)result.Result;

        Assert.NotNull(okResult.Value);
        Assert.NotEmpty(okResult.Value);

        Assert.True(okResult.Value.All(g => g.Creator.Id == userId));
    }

    [Theory]
    [InlineData("Test name 3", GameMode.Solo, 1)]
    [InlineData("Test name 4", GameMode.Duel, 2)]
    public async Task CreateUserGameCreatesUserGameInDatabase(
        string name,
        GameMode mode,
        int userId
    )
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);
        var initialGameCount = context.Games.Count();

        CreateGameDto createGameDto = new(name, mode, 1);
        CreateGameDtoValidator validator = new(context);

        var result = await GamesEndpoints.CreateUserGame(userId, createGameDto, validator, context);

        Assert.IsType<
            Results<
                Created<GameDto>,
                UnprocessableEntity<IEnumerable<CustomError>>,
                NotFound<CustomError>
            >
        >(result);

        var createdResult = (Created<GameDto>)result.Result;

        Assert.NotNull(createdResult);
        Assert.NotNull(createdResult.Location);

        var lastGame = context.Games.Last();
        var gameDiff = context.Games.Count() - initialGameCount;

        Assert.Equal(1, gameDiff);
        Assert.Equal(name, lastGame.Name);
        Assert.Equal(mode, lastGame.Mode);
        Assert.Equal(1, lastGame.Map.Id);
        Assert.Equal(userId, lastGame.Creator.Id);
    }

    [Theory]
    [InlineData(2, 2, 1)]
    public async Task UpdateUserGameUpdatesUserGameInDatabase(int userId, int gameId, int playerId)
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);
        var initialPlayerCount = context.Games.Single(g => g.Id == gameId).Players.Count();

        UpdateGameDto updateGameDto = new(playerId);
        UpdateGameDtoValidator validator = new();

        var result = await GamesEndpoints.UpdateUserGame(
            userId,
            gameId,
            updateGameDto,
            validator,
            context
        ); // Existing game should be updated

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

        var updatedGame = context.Games.Single(g => g.Id == gameId);
        var playerDiff = updatedGame.Players.Count() - initialPlayerCount;

        Assert.Equal(1, playerDiff);
        Assert.Contains(playerId, updatedGame.Players.Select(p => p.Id));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public async Task RemoveUserGameRemovesUserGameInDatabase(int userId, int gameId)
    {
        await using var context = new MockDb().CreateDbContext();
        await MockDb.SeedAsync(context);
        var initialGameCount = context.Games.Count();

        var result = await GamesEndpoints.RemoveUserGame(userId, gameId, context);

        Assert.IsType<Results<NoContent, NotFound<CustomError>>>(result);

        var noContentResult = (NoContent)result.Result;

        Assert.NotNull(noContentResult);

        var gameDiff = initialGameCount - context.Games.Count();

        Assert.Equal(1, gameDiff);
        Assert.DoesNotContain(gameId, context.Games.Select(g => g.Id));
    }
}
