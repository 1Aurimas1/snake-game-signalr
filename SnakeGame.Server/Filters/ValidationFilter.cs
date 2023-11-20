using FluentValidation;

namespace SnakeGame.Server.Filters;

public class ValidationFilter<T> : IEndpointFilter
    where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var obj = context.Arguments.FirstOrDefault(x => x?.GetType() == typeof(T)) as T;

        if (obj is null)
        {
            return TypedResults.BadRequest();
        }

        var validationResult = await _validator.ValidateAsync(obj);

        if (!validationResult.IsValid)
        {
            var responseResult = JsonResponseGenerator.GenerateFluentErrorResponse(
                validationResult.Errors
            );
            return TypedResults.UnprocessableEntity(responseResult);
        }

        return await next(context);
    }
}
