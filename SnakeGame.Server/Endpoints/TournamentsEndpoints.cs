using FluentValidation;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;

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

    public static async Task<IResult> GetAllTournaments(ITournamentService tournamentService)
    {
        var tournaments = await tournamentService.GetAll();

        return Results.Ok(tournaments.Select(x => x.ToDto()));
    }

    public static async Task<IResult> GetAllUserTournaments(
        int userId,
        IUserService userService,
        ITournamentService tournamentService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournaments = await tournamentService.GetAllFiltered(x => x.Organizer.Id == userId);

        return Results.Ok(tournaments.Select(x => x.ToDto()));
    }

    public static async Task<IResult> GetUserTournament(
        int userId,
        int id,
        IUserService userService,
        ITournamentService tournamentService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await tournamentService.GetUserTournament(userId, id);

        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        return Results.Ok(tournament.ToDto());
    }

    public static async Task<IResult> CreateUserTournament(
        int userId,
        CreateTournamentDto dto,
        IValidator<CreateTournamentDto> validator,
        IUserService userService,
        ITournamentService tournamentService
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(responseResult);
        }

        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await tournamentService.Add(dto, user);

        return Results.Created($"/users/{userId}/tournaments/{tournament.Id}", tournament.ToDto());
    }

    public static async Task<IResult> UpdateUserTournament(
        int userId,
        int id,
        UpdateTournamentDto dto,
        IValidator<UpdateTournamentDto> validator,
        IUserService userService,
        ITournamentService tournamentService
    )
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var response = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return Results.UnprocessableEntity(response);
        }

        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await tournamentService.GetUserTournament(userId, id);
        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        if (tournament.Participations.Count + 1 > tournament.MaxParticipants)
            return Results.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "tournament",
                    "Tournament is full"
                )
            );

        var participant = await userService.Get(dto.ParticipantId);
        if (participant == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("participant"));

        if (tournament.Participations.Exists(x => x.UserId == dto.ParticipantId))
            return Results.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "user",
                    "This user is already registered for tournament"
                )
            );

        await tournamentService.Update(participant, tournament);

        return Results.Ok(tournament.ToDto());
    }

    public static async Task<IResult> RemoveUserTournament(
        int userId,
        int id,
        IUserService userService,
        ITournamentService tournamentService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await tournamentService.GetUserTournament(userId, id);
        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        await tournamentService.Remove(tournament);

        return Results.NoContent();
    }

    public static async Task<IResult> GetAllUserTournamentRounds(
        int userId,
        int tournamentId,
        IUserService userService,
        ITournamentService tournamentService
    )
    {
        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await tournamentService.GetUserTournament(userId, tournamentId);
        if (tournament == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        var rounds = await tournamentService.GetAllTournamentRounds(tournament.Id);

        return Results.Ok(rounds.Select(x => x.ToDto()));
    }
}
