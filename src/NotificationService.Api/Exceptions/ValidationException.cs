using NotificationService.Api.Models;

namespace NotificationService.Api.Exceptions;

public class ValidationException : BaseDomainException
{
    public override string ErrorCode => "VALIDATION_ERROR";
    
    public Dictionary<string, List<ValidationError>> ValidationErrors { get; }

    public ValidationException(Dictionary<string, List<ValidationError>> validationErrors)
        : base($"Validation failed with {validationErrors.Count} error(s).")
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string message) : base(message)
    {
        ValidationErrors = [];
    }
}