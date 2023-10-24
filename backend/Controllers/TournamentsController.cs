using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using snake_game.Models;

namespace snake_game.Controllers;

[ApiController]
[Route("/api/users/{userId}/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly IValidator<CreateTournamentDto> _createValidator;
    private readonly IValidator<UpdateTournamentDto> _updateValidator;
    private readonly DataContext _context;

    public TournamentsController(
        IValidator<CreateTournamentDto> createValidator,
        IValidator<UpdateTournamentDto> updateValidator,
        DataContext context
    )
    {
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _context = context;
    }

    private IQueryable<Tournament> GetCompleteQuery()
    {
        return _context.Tournaments
            .Include(x => x.Organizer)
            .Include(x => x.Participations)
            .ThenInclude(x => x.User)
            .Include(x => x.Rounds)
            .ThenInclude(x => x.Map)
            .AsQueryable();
    }

    // All users endpoints

    [HttpGet("/api/[controller]")]
    public async Task<ActionResult<IEnumerable<TournamentDto>>> GetMany()
    {
        var tournaments = await GetCompleteQuery().ToListAsync();

        return Ok(tournaments.Select(x => x.ToDto()));
    }

    // Specific user endpoints

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentDto>>> GetMany(int userId)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournaments = await GetCompleteQuery()
            .Where(x => x.Organizer.Id == userId)
            .ToListAsync();

        return Ok(tournaments.Select(x => x.ToDto()));
    }

    [HttpGet("{tournamentId}")]
    public async Task<ActionResult<TournamentDto>> Get(int userId, int tournamentId)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await GetCompleteQuery()
            .SingleOrDefaultAsync(x => x.Id == tournamentId && x.Organizer.Id == userId);

        if (tournament == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        return tournament.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult> Create(int userId, CreateTournamentDto createTournamentDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(JsonResponseGenerator.GenerateModelErrorResponse(ModelState));

        var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var result = await _createValidator.ValidateAsync(createTournamentDto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return UnprocessableEntity(responseResult);
        }

        var tournament = createTournamentDto.FromCreateDto(user);
        _context.Tournaments.Add(tournament);

        await _context.SaveChangesAsync();

        return Created("", tournament.ToDto());
    }

    [HttpPatch("{tournamentId}")]
    public async Task<ActionResult<TournamentDto>> Update(
        int userId,
        int tournamentId,
        UpdateTournamentDto dto
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(JsonResponseGenerator.GenerateModelErrorResponse(ModelState));

        var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var result = await _updateValidator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var response = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return UnprocessableEntity(response);
        }

        var tournament = await GetCompleteQuery().SingleOrDefaultAsync(x => x.Id == tournamentId);
        if (tournament == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        if (tournament.Participations.Count + 1 > tournament.MaxParticipants)
            return UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "tournament",
                    "Tournament is full"
                )
            );

        var participant = await _context.Users.SingleOrDefaultAsync(x => x.Id == dto.ParticipantId);
        if (participant == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("participant"));

        if (tournament.Participations.Exists(x => x.UserId == dto.ParticipantId))
            return UnprocessableEntity(
                JsonResponseGenerator.GenerateUnprocessableEntityResponse(
                    "user",
                    "This user already registered for tournament"
                )
            );

        tournament.Participations.Add(
            new Participation { User = participant, Tournament = tournament }
        );

        await _context.SaveChangesAsync();

        return Ok(tournament.ToDto());
    }

    [HttpDelete("{tournamentId}")]
    public async Task<ActionResult> Remove(int userId, int tournamentId)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var tournament = await _context.Tournaments.SingleOrDefaultAsync(x => x.Id == tournamentId);
        if (tournament == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("tournament"));

        _context.Tournaments.Remove(tournament);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateTournamentDtoValidator : AbstractValidator<CreateTournamentDto>
{
    public CreateTournamentDtoValidator(DataContext dbContext)
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100);
        RuleFor(x => x.Rounds).NotEmpty();
        RuleForEach(x => x.Rounds).SetValidator(new CreateRoundDtoValidator(dbContext));
        RuleFor(x => x.MaxParticipants).NotEmpty().GreaterThan(1);
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty();
    }
}

public class UpdateTournamentDtoValidator : AbstractValidator<UpdateTournamentDto>
{
    public UpdateTournamentDtoValidator()
    {
        RuleFor(x => x.ParticipantId).NotEmpty();
    }
}

public class CreateRoundDtoValidator : AbstractValidator<CreateRoundDto>
{
    public CreateRoundDtoValidator(DataContext dbContext)
    {
        RuleFor(x => x.Index).NotEmpty().GreaterThan(0);
        RuleFor(x => x.MapId)
            .NotEmpty()
            .Must((mapId) => DoesMapExist(dbContext, mapId))
            .WithMessage("The map was not found.");
    }

    private bool DoesMapExist(DataContext dbContext, int mapId)
    {
        return dbContext.Maps.Any(x => x.Id == mapId);
    }
}
