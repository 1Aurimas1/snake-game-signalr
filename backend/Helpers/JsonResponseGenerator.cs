using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public static class JsonResponseGenerator
{
    public static string GenerateFluentErrorResponse(
        List<FluentValidation.Results.ValidationFailure> validationFailures
    )
    {
        var errors = validationFailures.Select(
            x => new CustomErrorResponse(x.PropertyName, x.ErrorCode, x.ErrorMessage)
        );

        var responseResult = JsonSerializer.Serialize(errors);
        return responseResult;
    }

    public static string GenerateModelErrorResponse(ModelStateDictionary modelDictionary)
    {
        var errors = modelDictionary.Values.Select(
            v => v.Errors.Select(e => new CustomErrorResponse("", "", e.ErrorMessage))
        );

        var responseResult = JsonSerializer.Serialize(errors);
        return responseResult;
    }

    public static string GenerateNotFoundResponse(string propertyName)
    {
        var error = new CustomErrorResponse(
            propertyName,
            "Not found",
            $"Resource '{propertyName}' not found"
        );

        var responseResult = JsonSerializer.Serialize(error);
        return responseResult;
    }

    public static string GenerateUnprocessableEntityResponse(
        string propertyName,
        string errorMessage
    )
    {
        var error = new CustomErrorResponse(propertyName, "Unprocessable entity", errorMessage);

        var responseResult = JsonSerializer.Serialize(error);
        return responseResult;
    }
}

public record CustomErrorResponse(string PropertyName, string ErrorCode, string ErrorMessage);
