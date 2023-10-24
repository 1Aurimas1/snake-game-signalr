using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using snake_game.Models;

namespace snake_game.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class MapsController : ControllerBase
{
    private readonly IValidator<CreateMapDto> _createValidator;
    private readonly IValidator<UpdateMapDto> _updateValidator;
    private readonly DataContext _context;

    public MapsController(
        IValidator<CreateMapDto> createValidator,
        IValidator<UpdateMapDto> updateValidator,
        DataContext context
    )
    {
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _context = context;
    }

    private IQueryable<Map> GetCompleteQuery()
    {
        return _context.Maps
            .Include(x => x.Creator)
            .Include(x => x.MapObstacles)
            .ThenInclude(x => x.Position)
            .AsQueryable();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MapDto>>> GetMany()
    {
        var maps = await GetCompleteQuery()
            .ToListAsync();

        return Ok(maps.Select(x => x.ToDto()));
    }

    [HttpGet("{mapId}")]
    public async Task<ActionResult<MapDto>> Get(int mapId)
    {
        var map = await GetCompleteQuery().SingleOrDefaultAsync(x => x.Id == mapId);

        if (map == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        return Ok(map.ToDto());
    }

    [HttpPost("/api/users/{userId}/[controller]")]
    public async Task<ActionResult<MapDto>> Create(int userId, CreateMapDto createMapDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(JsonResponseGenerator.GenerateModelErrorResponse(ModelState));

        var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        var result = await _createValidator.ValidateAsync(createMapDto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return UnprocessableEntity(responseResult);
        }

        var uniqueObstacleIds = createMapDto.MapObstacles
            .Select(x => x.ObstacleId)
            .Distinct()
            .ToList();
        var uniqueObstacles = _context.Obstacles
            .Where(x => uniqueObstacleIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x);

        List<MapObstacle> mapObstacles = new();
        foreach (var mapObstacleDto in createMapDto.MapObstacles)
            if (uniqueObstacles.ContainsKey(mapObstacleDto.ObstacleId))
                mapObstacles.Add(mapObstacleDto.FromCreateDto());

        var map = createMapDto.FromCreateDto(mapObstacles, user);
        _context.Maps.Add(map);

        await _context.SaveChangesAsync();

        return Created("", map.ToDto());
    }

    [HttpPatch("{mapId}")]
    public async Task<ActionResult<MapDto>> Update(int mapId, UpdateMapDto updateMapDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(JsonResponseGenerator.GenerateModelErrorResponse(ModelState));

        var result = await _updateValidator.ValidateAsync(updateMapDto);
        if (!result.IsValid)
        {
            var response = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return UnprocessableEntity(response);
        }

        // TODO: different queries?
        var map = await GetCompleteQuery()
            .Include(x => x.MapRatings)
            .SingleOrDefaultAsync(x => x.Id == mapId);
        if (map == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        // TODO: unique userId and mapId in MapRating table
        var mapRating = map.MapRatings.SingleOrDefault(x => x.UserId == updateMapDto.UserId);
        if (mapRating == null)
        {
            mapRating = new MapRating
            {
                Rating = updateMapDto.MapRating,
                UserId = updateMapDto.UserId,
            };
            map.MapRatings.Add(mapRating);
        }
        else
        {
            mapRating.Rating = updateMapDto.MapRating;
        }

        var newRating = (double)map.MapRatings.Sum(x => x.Rating) / map.MapRatings.Count;

        // TODO?: if user gets deleted update rating
        map.Rating = newRating;

        await _context.SaveChangesAsync();

        return Ok(map.ToDto());
    }

    [HttpDelete("{mapId}")]
    public async Task<ActionResult> Remove(int mapId)
    {
        var map = await _context.Maps.SingleOrDefaultAsync(x => x.Id == mapId);
        if (map == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("map"));

        _context.Maps.Remove(map);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateMapDtoValidator : AbstractValidator<CreateMapDto>
{
    public CreateMapDtoValidator(DataContext dbContext)
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100);
        RuleFor(x => x.MapObstacles).NotNull();
        RuleForEach(x => x.MapObstacles).SetValidator(new CreateMapObstacleDtoValidator(dbContext));
    }
}

public class CreateMapObstacleDtoValidator : AbstractValidator<CreateMapObstacleDto>
{
    public CreateMapObstacleDtoValidator(DataContext dbContext)
    {
        RuleFor(x => x.ObstacleId)
            .NotNull()
            .Must((obstacleId) => DoesObstacleExist(dbContext, obstacleId))
            .WithMessage("The obstacle was not found.");
        RuleFor(x => x.Position).NotEmpty().SetValidator(new CreatePointDtoValidator());
    }

    private bool DoesObstacleExist(DataContext dbContext, int obstacleId)
    {
        return dbContext.Obstacles.Any(x => x.Id == obstacleId);
    }
}

public class CreatePointDtoValidator : AbstractValidator<CreatePointDto>
{
    public CreatePointDtoValidator()
    {
        RuleFor(x => x.Y).NotNull();
        RuleFor(x => x.X).NotNull();
    }
}

public class UpdateMapDtoValidator : AbstractValidator<UpdateMapDto>
{
    public UpdateMapDtoValidator(DataContext dbContext)
    {
        RuleFor(x => x.MapRating).NotEmpty().InclusiveBetween(1, 5);
        RuleFor(x => x.UserId)
            .NotNull()
            .Must((userId) => DoesUserExist(dbContext, userId))
            .WithMessage("The user was not found.");
    }

    private bool DoesUserExist(DataContext dbContext, int userId)
    {
        return dbContext.Users.Any(x => x.Id == userId);
    }
}
