using System.Linq.Expressions;
using SnakeGame.Server.Models;

namespace SnakeGame.Server.Services.DataServices;

public interface IGameService
{
    Task<List<Game>> GetAll();
    Task<List<Game>> GetAllFiltered(Expression<Func<Game, bool>> predicate);
    Task<Game?> GetUserGame(int userId, int id);
    Task Add(Game game);
    Task Update(Game game, User player);
    Task Remove(Game game);
}

public class GameService : IGameService
{
    private readonly DataContext _dbContext;

    public GameService(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Game>> GetAll()
    {
        return await _dbContext.Games.WithIncludes().ToListAsync();
    }

    public async Task<List<Game>> GetAllFiltered(Expression<Func<Game, bool>> predicate)
    {
        return await _dbContext.Games.WithIncludes().Where(predicate).ToListAsync();
    }

    public async Task<Game?> GetUserGame(int userId, int id)
    {
        return await _dbContext.Games
            .WithIncludes()
            .FirstOrDefaultAsync(g => g.Creator.Id == userId && g.Id == id);
    }

    public async Task Add(Game game)
    {
        await _dbContext.Games.AddAsync(game);

        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(Game game, User player)
    {
        game.IsOpen = game.Mode switch
        {
            GameMode.Duel or GameMode.Solo => false,
            _ => throw new NotImplementedException("Add new GameMode enum members"),
        };
        game.Players.Add(player);

        await _dbContext.SaveChangesAsync();
    }

    public async Task Remove(Game game)
    {
        _dbContext.Games.Remove(game);

        await _dbContext.SaveChangesAsync();
    }
}

public static class GameQueryExtensions
{
    public static IQueryable<Game> WithIncludes(this IQueryable<Game> query)
    {
        return query.Include(x => x.Map).Include(x => x.Creator).Include(x => x.Players);
    }
}
