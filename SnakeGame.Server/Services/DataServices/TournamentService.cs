using System.Linq.Expressions;
using SnakeGame.Server.Models;

namespace SnakeGame.Server.Services.DataServices;

public interface ITournamentService
{
    Task<List<Tournament>> GetAll();
    Task<List<Tournament>> GetAllFiltered(Expression<Func<Tournament, bool>> predicate);
    Task<List<Round>> GetAllTournamentRounds(int id);
    Task<Tournament?> GetUserTournament(int userId, int id);
    Task<Tournament> Add(CreateTournamentDto dto, User organizer);
    Task Update(User participant, Tournament tournament);
    Task Remove(Tournament tournament);
}

public class TournamentService : ITournamentService
{
    private readonly DataContext _dbContext;

    public TournamentService(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Tournament>> GetAll()
    {
        return await _dbContext.Tournaments.WithIncludes().ToListAsync();
    }

    public async Task<List<Tournament>> GetAllFiltered(Expression<Func<Tournament, bool>> predicate)
    {
        return await _dbContext.Tournaments.WithIncludes().Where(predicate).ToListAsync();
    }

    public async Task<List<Round>> GetAllTournamentRounds(int id)
    {
        return await _dbContext.Rounds
            .Include(x => x.Map)
            .ThenInclude(x => x.MapRatings)
            .Include(x => x.Map)
            .ThenInclude(x => x.MapObstacles)
            .Include(x => x.Map)
            .ThenInclude(x => x.Creator)
            .Where(x => x.TournamentId == id)
            .ToListAsync();
    }

    public async Task<Tournament?> GetUserTournament(int userId, int id)
    {
        return await _dbContext.Tournaments
            //.Include(x => x.Organizer) // TODO: what if not all includes needed
            .WithIncludes()
            .FirstOrDefaultAsync(x => x.Organizer.Id == userId && x.Id == id);
    }

    public async Task<Tournament> Add(CreateTournamentDto dto, User organizer)
    {
        var tournament = dto.FromCreateDto(organizer);
        await _dbContext.Tournaments.AddAsync(tournament);

        await _dbContext.SaveChangesAsync();

        return tournament;
    }

    public async Task Update(User participant, Tournament tournament)
    {
        tournament.Participations.Add(
            new Participation { User = participant, Tournament = tournament }
        );

        await _dbContext.SaveChangesAsync();
    }

    public async Task Remove(Tournament tournament)
    {
        _dbContext.Tournaments.Remove(tournament);

        await _dbContext.SaveChangesAsync();
    }
}

public static class TournamentQueryExtensions
{
    public static IQueryable<Tournament> WithIncludes(this IQueryable<Tournament> query)
    {
        return query
            .Include(x => x.Organizer)
            .Include(x => x.Participations)
            .ThenInclude(x => x.User)
            .Include(x => x.Rounds)
            .ThenInclude(x => x.Map);
    }
}
