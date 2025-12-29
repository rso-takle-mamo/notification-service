namespace NotificationService.Api.Exceptions;

public class ConflictException : BaseDomainException
{
    public override string ErrorCode => "CONFLICT";
    
    public string ConflictType { get; }

    public ConflictException(string message) : base(message) { }

    public ConflictException(string conflictType, string message) : base(message)
    {
        ConflictType = conflictType;
    }

    public ConflictException(string message, Exception innerException) : base(message, innerException) { }

    public ConflictException(string conflictType, string message, Exception innerException) : base(message, innerException)
    {
        ConflictType = conflictType;
    }
}