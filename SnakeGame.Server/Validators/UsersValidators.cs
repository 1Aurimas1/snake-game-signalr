using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SnakeGame.Server.Models;

namespace SnakeGame.Server.Validators.UsersValidators;

public class BaseUserDtoValidator<T> : AbstractValidator<T>
    where T : BaseUserDto
{
    private readonly UserManager<User> _userManager;

    public BaseUserDtoValidator(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    protected async Task<bool> IsNameUnique(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        return user == null ? true : false;
    }

    protected async Task<bool> IsEmailUnique(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user == null ? true : false;
    }
}

public class RegisterUserDtoValidator : BaseUserDtoValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator(UserManager<User> userManager)
        : base(userManager)
    {
        RuleFor(x => x.UserName).ValidateUsernameFull(IsNameUnique);
        RuleFor(x => x.Email).ValidateUsernameFull(IsEmailUnique);
        RuleFor(x => x.Password)
            .ValidatePasswordBasic()
            .Must((model, field) => field == model.PasswordConfirmation)
            .WithMessage("Passwords must match");
    }
}

public class UpdateUserDtoValidator : BaseUserDtoValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator(UserManager<User> userManager)
        : base(userManager)
    {
        RuleFor(x => x.UserName)
            .ValidateUsernameFull(IsNameUnique)
            .When(x => string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Email)
            .ValidateEmail(IsEmailUnique)
            .When(x => string.IsNullOrEmpty(x.UserName));
    }
}

public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDtoValidator(UserManager<User> userManager)
    {
        RuleFor(x => x.UserName).ValidateUsernameBasic();
        RuleFor(x => x.Password).ValidatePasswordBasic();
    }
}

public static class UserValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> ValidateUsernameBasic<T>(
        this IRuleBuilder<T, string> ruleBuilder
    )
    {
        return ruleBuilder.NotEmpty().Length(5, 15);
    }

    public static IRuleBuilderOptions<T, string> ValidateUsernameFull<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Func<string, Task<bool>> IsUnique
    )
    {
        return ruleBuilder
            .ValidateUsernameBasic()
            .MustAsync(async (username, cancellation) => await IsUnique(username))
            .WithMessage("The username is already in use.");
    }

    public static IRuleBuilderOptions<T, string> ValidateEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Func<string, Task<bool>> IsUnique
    )
    {
        return ruleBuilder
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, cancellation) => await IsUnique(email))
            .WithMessage("The email is already in use.");
    }

    public static IRuleBuilderOptions<T, string> ValidatePasswordBasic<T>(
        this IRuleBuilder<T, string> ruleBuilder
    )
    {
        return ruleBuilder.NotEmpty().Length(6, 30);
    }
}
