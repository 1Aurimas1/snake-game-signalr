using FluentValidation;
using SnakeGame.Server.Models;

public static class TournamentsEndpoints
{
    public static RouteGroupBuilder MapTournamentsApi(this RouteGroupBuilder group)
    {
        // Tournaments
        group.MapGet("/tournaments", GetAll);
        group.MapGet("/users/{userId}/tournaments", GetMany);
        group.MapGet("/users/{userId}/tournaments/{id}", Get);
        group.MapPost("/users/{userId}/tournaments", Create);
        group.MapPatch("/users/{userId}/tournaments/{id}", Update);
        group.MapDelete("/users/{userId}/tournaments/{id}", Remove);
        // Rounds
        group.MapGet(
            "/users/{userId}/tournaments/{tournamentId}/rounds",
            GetManyRoundsByUserByTournament
        );

        return group;
    }

    private static IQueryable<Tournament> GetCompleteQuery(DataContext dbContext)
    {
        return dbContext.Tournaments
            .Include(x => x.Organizer)
            .Include(x => x.Participations)
            .ThenInclude(x => x.User)
            .Include(x => x.Rounds)
            .ThenInclude(x => x.Map)
            .AsQueryable();
    }

    // Nonspecific endpoints

    public static async Task<IResult> GetAll(DataContext dbContext)
    {
        var tournaments = await GetCompleteQuery(dbContext).ToListAsync();

        return Results.Ok(tournaments.Select(x => x.ToDto()));
    }

    // Specific user endpoints

    public static async Task<IResult> GetMany(int userId, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournaments = await GetCompleteQuery(dbContext)
            .Where(x => x.Organizer.Id == userId)
            .ToListAsync();

        return Results.Ok(tournaments.Select(x => x.ToDto()));
    }

    public static async Task<IResult> Get(int userId, int id, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await GetCompleteQuery(dbContext)
            .FirstOrDefaultAsync(x => x.Id == id && x.Organizer.Id == userId);

        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        return Results.Ok(tournament.ToDto());
    }

    public static async Task<IResult> Create(
        int userId,
        CreateTournamentDto dto,
        IValidator<CreateTournamentDto> validator,
        DataContext dbContext
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = dto.FromCreateDto(user);
        dbContext.Tournaments.Add(tournament);

        await dbContext.SaveChangesAsync();

        return Results.Created($"/users/{userId}/tournaments/{tournament.Id}", tournament.ToDto());
    }

    public static async Task<IResult> Update(
        int userId,
        int id,
        UpdateTournamentDto dto,
        IValidator<UpdateTournamentDto> validator,
        DataContext dbContext
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var response = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(response);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await GetCompleteQuery(dbContext)
            .FirstOrDefaultAsync(x => x.Id == id && x.Organizer.Id == userId);
        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        if (tournament.Participations.Count + 1 > tournament.MaxParticipants)
            return Results.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "tournament",
                    "Tournament is full"
                )
            );

        var participant = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == dto.ParticipantId);
        if (participant == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("participant"));

        if (tournament.Participations.Exists(x => x.UserId == dto.ParticipantId))
            return Results.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "user",
                    "This user is already registered for tournament"
                )
            );

        tournament.Participations.Add(
            new Participation { User = participant, Tournament = tournament }
        );

        await dbContext.SaveChangesAsync();

        return Results.Ok(tournament.ToDto());
    }

    public static async Task<IResult> Remove(int userId, int id, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await dbContext.Tournaments.FirstOrDefaultAsync(
            x => x.Id == id && x.Organizer.Id == userId
        );
        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        dbContext.Tournaments.Remove(tournament);
        await dbContext.SaveChangesAsync();

        return Results.NoContent();
    }

    public static async Task<IResult> GetManyRoundsByUserByTournament(
        int userId,
        int tournamentId,
        DataContext dbContext
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await dbContext.Tournaments.FirstOrDefaultAsync(
            x => x.Id == tournamentId && x.Organizer.Id == userId
        );
        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        var rounds = await dbContext.Rounds
            .Where(x => x.TournamentId == tournamentId)
            .ToListAsync();

        return Results.Ok(rounds.Select(x => x.ToDto()));
    }
}
