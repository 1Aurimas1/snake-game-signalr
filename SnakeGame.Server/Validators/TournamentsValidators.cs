using FluentValidation;
using SnakeGame.Server.Models;

public class CreateTournamentDtoValidator : AbstractValidator<CreateTournamentDto>
{
    public CreateTournamentDtoValidator(DataContext dbContext)
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 30);
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
