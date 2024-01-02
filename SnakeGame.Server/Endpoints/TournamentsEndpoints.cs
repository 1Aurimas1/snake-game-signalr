using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using SnakeGame.Server.Filters;
using SnakeGame.Server.Helpers;
using SnakeGame.Server.Models;
using SnakeGame.Server.Services.DataServices;

public static class TournamentsEndpoints
{
    public static RouteGroupBuilder MapTournamentsApi(this RouteGroupBuilder group)
    {
        // Tournaments
        group.MapGet("/tournaments", GetAllTournaments);
        group
            .MapGet("/users/{userId}/tournaments/{id}", GetUserTournament)
            .WithName(nameof(GetUserTournament));
        group
            .MapPost("/tournaments", CreateUserTournament)
            .AddEndpointFilter<ValidationFilter<CreateTournamentDto>>();
        group.MapPatch("/users/{userId}/tournaments/{id}", UpdateUserTournament);
        group.MapDelete("/users/{userId}/tournaments/{id}", RemoveUserTournament);

        group.MapGet("/users/{userId}/tournaments", GetAllUserTournaments);
        // Rounds
        group.MapGet(
            "/users/{userId}/tournaments/{tournamentId}/rounds",
            GetAllUserTournamentRounds
        );

        return group;
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> GetAllTournaments(ITournamentService tournamentService)
    {
        var tournaments = await tournamentService.GetAll();

        return Results.Ok(tournaments.Select(x => x.ToDto()));
    }

    [Authorize(Roles = UserRoles.Basic)]
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

    [Authorize(Roles = UserRoles.Admin)]
    public static async Task<IResult> CreateUserTournament(
        HttpContext httpContext,
        CreateTournamentDto dto,
        IUserService userService,
        ITournamentService tournamentService
    )
    {
        if (!httpContext.TryGetJwtUserId(out int userId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "UserId",
                    "Invalid user ID"
                )
            );

        var user = await userService.Get(userId);
        if (user == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await tournamentService.Add(dto, user);

        return TypedResults.CreatedAtRoute(
            tournament.ToDto(),
            nameof(GetUserTournament),
            new { userId = userId, id = tournament.Id }
        );
    }

    [Authorize(Roles = UserRoles.Basic)]
    public static async Task<IResult> UpdateUserTournament(
        int userId,
        int id,
        HttpContext httpContext,
        IUserService userService,
        ITournamentService tournamentService
    )
    {
        if (!httpContext.TryGetJwtUserId(out int participantId))
            return TypedResults.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "participantId",
                    "Invalid participant ID"
                )
            );

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

        var participant = await userService.Get(participantId);
        if (participant == null)
            return Results.NotFound(JsonResponseGenerator.GenerateNotFoundResponse("participant"));

        if (tournament.Participations.Exists(x => x.UserId == participantId))
            return Results.UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "user",
                    "This user is already registered for tournament"
                )
            );

        await tournamentService.Update(participant, tournament);

        return Results.Ok(tournament.ToDto());
    }

    [Authorize(Roles = UserRoles.Admin)]
    public static async Task<IResult> RemoveUserTournament(
        int userId,
        HttpContext httpContext,
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

    [Authorize(Roles = UserRoles.Basic)]
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

    [Authorize(Roles = UserRoles.Admin)]
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
