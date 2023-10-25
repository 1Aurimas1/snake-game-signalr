using FluentValidation;
using snake_game.Models;

public class CreateMapDtoValidator : AbstractValidator<CreateMapDto>
{
    public CreateMapDtoValidator(DataContext dbContext)
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 15);
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
        RuleFor(x => x.MapRating).NotEmpty().NotNull().InclusiveBetween(1, 5);
        RuleFor(x => x.UserId)
            .NotEmpty()
            .NotNull()
            .Must((userId) => DoesUserExist(dbContext, userId))
            .WithMessage("The user was not found.");
    }

    private bool DoesUserExist(DataContext dbContext, int userId)
    {
        return dbContext.Users.Any(x => x.Id == userId);
    }
}
