using FluentValidation;
using snake_game.Models;

public static class TournamentsEndpoints
{
    public static RouteGroupBuilder MapTournamentsApi(this RouteGroupBuilder group)
    {
        group.MapGet("/tournaments", GetMany);
        group.MapGet("/users/{userId}/tournaments", GetManyByUser);
        group.MapGet("/users/{userId}/tournaments/{id}", Get);
        group.MapPost("/tournaments", Create);
        group.MapPatch("/tournaments/{id}", Update);
        group.MapDelete("/tournaments/{id}", Remove);

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

    // All users endpoints

    public static async Task<IResult> GetMany(DataContext dbContext)
    {
        var tournaments = await GetCompleteQuery(dbContext).ToListAsync();

        return Results.Ok(tournaments.Select(x => x.ToDto()));
    }

    // Specific user endpoints

    public static async Task<IResult> GetManyByUser(int userId, DataContext dbContext)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournaments = await GetCompleteQuery(dbContext)
            .Where(x => x.Organizer.Id == userId)
            .ToListAsync();

        return Results.Ok(tournaments.Select(x => x.ToDto()));
    }

    public static async Task<IResult> Get(int userId, int id, DataContext dbContext)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await GetCompleteQuery(dbContext)
            .SingleOrDefaultAsync(x => x.Id == id && x.Organizer.Id == userId);

        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        return Results.Ok(tournament.ToDto());
    }

    public static async Task<IResult> Create(int userId, CreateTournamentDto dto, IValidator<CreateTournamentDto> validator, DataContext dbContext)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

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
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var response = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(response);
        }

        var tournament = await GetCompleteQuery(dbContext).SingleOrDefaultAsync(x => x.Id == id);
        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        if (tournament.Participations.Count + 1 > tournament.MaxParticipants)
            return Results.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "tournament",
                    "Tournament is full"
                )
            );

        var participant = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == dto.ParticipantId);
        if (participant == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("participant"));

        if (tournament.Participations.Exists(x => x.UserId == dto.ParticipantId))
            return Results.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "user",
                    "This user already registered for tournament"
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
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await dbContext.Tournaments.SingleOrDefaultAsync(x => x.Id == id);
        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        dbContext.Tournaments.Remove(tournament);
        await dbContext.SaveChangesAsync();

        return Results.NoContent();
    }
}
