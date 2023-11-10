using FluentValidation;
using SnakeGame.Server.Models;

public static class TournamentsEndpoints
{
    public static RouteGroupBuilder MapTournamentsApi(this RouteGroupBuilder group)
    {
        // Tournaments
        group.MapGet("/tournaments", GetAllTournaments);
        group.MapGet("/users/{userId}/tournaments", GetAllUserTournaments);
        group.MapGet("/users/{userId}/tournaments/{id}", GetUserTournament);
        group.MapPost("/users/{userId}/tournaments", CreateUserTournament);
        group.MapPatch("/users/{userId}/tournaments/{id}", UpdateUserTournament);
        group.MapDelete("/users/{userId}/tournaments/{id}", RemoveUserTournament);
        // Rounds
        group.MapGet(
            "/users/{userId}/tournaments/{tournamentId}/rounds",
            GetAllUserTournamentRounds
        );

        return group;
    }

    private static IQueryable<Tournament> WithIncludes(this IQueryable<Tournament> query)
    {
        return query
            .Include(x => x.Organizer)
            .Include(x => x.Participations)
            .ThenInclude(x => x.User)
            .Include(x => x.Rounds)
            .ThenInclude(x => x.Map);
    }

    public static async Task<IResult> GetAllTournaments(DataContext dbContext)
    {
        var tournaments = await dbContext.Tournaments.WithIncludes().ToListAsync();

        return Results.Ok(tournaments.Select(x => x.ToDto()));
    }

    public static async Task<IResult> GetAllUserTournaments(int userId, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournaments = await dbContext.Tournaments
            .WithIncludes()
            .Where(x => x.Organizer.Id == userId)
            .ToListAsync();

        return Results.Ok(tournaments.Select(x => x.ToDto()));
    }

    public static async Task<IResult> GetUserTournament(int userId, int id, DataContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await dbContext.Tournaments
            .WithIncludes()
            .FirstOrDefaultAsync(x => x.Id == id && x.Organizer.Id == userId);

        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        return Results.Ok(tournament.ToDto());
    }

    public static async Task<IResult> CreateUserTournament(
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

    public static async Task<IResult> UpdateUserTournament(
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

        var tournament = await dbContext.Tournaments
            .WithIncludes()
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

    public static async Task<IResult> RemoveUserTournament(
        int userId,
        int id,
        DataContext dbContext
    )
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

    public static async Task<IResult> GetAllUserTournamentRounds(
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
