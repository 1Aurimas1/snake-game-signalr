using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SnakeGame.Server.Models;
using SnakeGame.Server.Auth.Services.TokenServices;
using SnakeGame.Tests.Helpers;

public class GamesIntegrationTests
{
    [Theory]
    [InlineData("Test name 3", GameMode.Solo, 1, 1)]
    [InlineData("Test name 4", GameMode.Duel, 2, 1)]
    public async Task Post_CreateUserGameReturnsSuccess(
        string name,
        GameMode mode,
        int userId,
        int mapId
    )
    {
        await using var application = new SnakeGameApplication();
        using var client = application.CreateClient();
        using var scope = application.Services.CreateScope();

        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var accessToken = jwtTokenService.CreateAccessToken(
            name,
            userId.ToString(),
            new List<string> { UserRoles.Basic }
        );

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );
        CreateGameDto dto = new(name, mode, mapId);

        var content = new StringContent(
            JsonSerializer.Serialize(dto),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync("/api/v1/games", content);

        response.EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdGame = await response.Content.ReadFromJsonAsync<GameDto>();

        Assert.NotNull(createdGame);

        Assert.Equal(name, createdGame.Name);
    }
}
