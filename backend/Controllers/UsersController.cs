using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using snake_game.Models;

namespace snake_game.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<UpdateUserDto> _updateValidator;
    private readonly DataContext _context;

    public UsersController(
        IValidator<RegisterUserDto> registerValidator,
        IValidator<UpdateUserDto> updateValidator,
        DataContext context
    )
    {
        _registerValidator = registerValidator;
        _updateValidator = updateValidator;
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<UserDto>> GetMany()
    {
        var users = await _context.Users.ToListAsync();

        return users.Select(x => x.ToDto());
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> Get(int userId)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        return user.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult> Register(RegisterUserDto registerUserDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(JsonResponseGenerator.GenerateModelErrorResponse(ModelState));

        var result = await _registerValidator.ValidateAsync(registerUserDto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);
            return UnprocessableEntity(responseResult);
        }

        var user = registerUserDto.FromRegisterDto();
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Created("", user.ToDto());
    }

    [HttpPatch("{userId}")]
    public async Task<ActionResult<UserDto>> Update(int userId, UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(JsonResponseGenerator.GenerateModelErrorResponse(ModelState));

        var result = await _updateValidator.ValidateAsync(updateUserDto);
        if (!result.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(result.Errors);

            return UnprocessableEntity(responseResult);
        }

        var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        user.UpdateWithDto(updateUserDto);

        await _context.SaveChangesAsync();

        return Ok(user.ToDto());
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult> Remove(int userId)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound(JsonResponseGenerator.GenerateNotFoundResponse("user"));

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class BaseUserDtoValidator<T> : AbstractValidator<T>
    where T : BaseUserDto
{
    public BaseUserDtoValidator(DataContext dbContext)
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(5, 15)
            .Must((user, username) => IsNameUnique(dbContext, username))
            .WithMessage("The username must be unique.");
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .Must((user, email) => IsEmailUnique(dbContext, email))
            .WithMessage("The email must be unique.");
    }

    private bool IsNameUnique(DataContext dbContext, string username)
    {
        return !dbContext.Users.Any(x => x.Username == username);
    }

    private bool IsEmailUnique(DataContext dbContext, string email)
    {
        return !dbContext.Users.Any(x => x.Email == email);
    }
}

public class CreateUserDtoValidator : BaseUserDtoValidator<RegisterUserDto>
{
    public CreateUserDtoValidator(DataContext dbContext)
        : base(dbContext)
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .NotNull()
            .Length(6, 30)
            .Must((model, field) => field == model.PasswordConfirmation)
            .WithMessage("Passwords must match");
    }
}

public class UpdateUserDtoValidator : BaseUserDtoValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator(DataContext dbContext)
        : base(dbContext) { }
}
