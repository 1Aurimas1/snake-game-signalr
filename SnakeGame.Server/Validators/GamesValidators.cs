using FluentValidation;
using SnakeGame.Server.Models;

public class CreateGameDtoValidator : AbstractValidator<CreateGameDto>
{
    public CreateGameDtoValidator(DataContext dbContext)
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 30);
        RuleFor(x => x.Mode).IsInEnum();
        RuleFor(x => x.MapId).NotNull();
    }
}

public class UpdateGameDtoValidator : AbstractValidator<UpdateGameDto>
{
    public UpdateGameDtoValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}
