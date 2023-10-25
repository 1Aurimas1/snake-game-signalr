using Microsoft.AspNetCore.Mvc.ModelBinding;

public static class JsonResponseGenerator
{
    public static IEnumerable<CustomError> GenerateFluentErrorResponse(
        List<FluentValidation.Results.ValidationFailure> validationFailures
    )
    {
        var errors = validationFailures.Select(
            x => new CustomError(x.PropertyName, x.ErrorCode, x.ErrorMessage)
        );

        return errors;
    }

    public static IEnumerable<IEnumerable<CustomError>> GenerateModelErrorResponse(ModelStateDictionary modelDictionary)
    {
        var errors = modelDictionary.Values.Select(
            v => v.Errors.Select(e => new CustomError("", "", e.ErrorMessage))
        );

        return errors;
    }

    public static CustomError GenerateNotFoundResponse(string propertyName)
    {
        var error = new CustomError(
            propertyName,
            "Not found",
            $"Resource '{propertyName}' not found"
        );

        return error;
    }

    public static CustomError GenerateUnprocessableEntityResponse(
        string propertyName,
        string errorMessage
    )
    {
        var error = new CustomError(propertyName, "Unprocessable entity", errorMessage);

        return error;
    }

    public static CustomError GenerateExceptionResponse(string exception, string exceptionMessage)
    {
        string propertyName = GetPropertyName(exceptionMessage);

        string errorMessage =
            propertyName == string.Empty
                ? exceptionMessage
                : $"Invalid '{propertyName}' field value";
        var error = new CustomError(propertyName, exception, errorMessage);

        return error;
    }

    private static string GetPropertyName(string message)
    {
        string propertyName = string.Empty;

        int start = message.IndexOf("$.");
        if (start > 0)
        {
            start += 2;
            int end = message.IndexOf(" ", start);
            propertyName = message.Substring(start, end - start);
        }

        return propertyName;
    }
}

public record CustomError(string PropertyName, string ErrorCode, string ErrorMessage);
