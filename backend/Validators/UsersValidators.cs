using FluentValidation;
using snake_game.Models;

public class BaseUserDtoValidator<T> : AbstractValidator<T>
    where T : BaseUserDto
{
    public BaseUserDtoValidator(DataContext dbContext) { }

    protected bool IsNameUnique(DataContext dbContext, string username)
    {
        return !dbContext.Users.Any(x => x.Username == username);
    }

    protected bool IsEmailUnique(DataContext dbContext, string email)
    {
        return !dbContext.Users.Any(x => x.Email == email);
    }
}

public class CreateUserDtoValidator : BaseUserDtoValidator<CreateUserDto>
{
    public CreateUserDtoValidator(DataContext dbContext)
        : base(dbContext)
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(5, 15)
            .Must((user, username) => IsNameUnique(dbContext, username))
            .WithMessage("The username is already in use.");
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .Must((user, email) => IsEmailUnique(dbContext, email))
            .WithMessage("The email is already in use.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(6, 30)
            .Must((model, field) => field == model.PasswordConfirmation)
            .WithMessage("Passwords must match");
    }
}

public class UpdateUserDtoValidator : BaseUserDtoValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator(DataContext dbContext)
        : base(dbContext)
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(5, 15)
            .Must((user, username) => IsNameUnique(dbContext, username))
            .WithMessage("The username is already in use.")
            .When(x => string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .Must((user, email) => IsEmailUnique(dbContext, email))
            .WithMessage("The email is already in use.")
            .When(x => string.IsNullOrEmpty(x.Username));
    }
}

public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDtoValidator(DataContext dbContext)
    {
        RuleFor(x => x.Username).NotEmpty().Length(5, 15);
        RuleFor(x => x.Password).NotEmpty().Length(6, 30);
    }
}
