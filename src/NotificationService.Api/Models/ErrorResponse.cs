namespace NotificationService.Api.Models;

public class ErrorResponse
{
    public required Error Error { get; set; }
}

public class ValidationErrorResponse : ErrorResponse
{
    public required Dictionary<string, List<ValidationError>> ValidationErrors { get; set; }
}

public static class ErrorResponses
{
    public static ErrorResponse Create(string code, string message)
    {
        return new ErrorResponse
        {
            Error = new Error
            {
                Code = code,
                Message = message
            }
        };
    }

    public static ErrorResponse Create(string code, string message, string resourceType, object? resourceId)
    {
        return new ErrorResponse
        {
            Error = new Error
            {
                Code = code,
                Message = message,
                ResourceType = resourceType,
                ResourceId = resourceId
            }
        };
    }

    public static ValidationErrorResponse CreateValidation(string message, Dictionary<string, List<ValidationError>> validationErrors)
    {
        return new ValidationErrorResponse
        {
            Error = new Error
            {
                Code = "VALIDATION_ERROR",
                Message = message
            },
            ValidationErrors = validationErrors
        };
    }
}