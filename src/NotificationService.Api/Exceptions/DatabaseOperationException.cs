namespace NotificationService.Api.Exceptions;

public class DatabaseOperationException : BaseDomainException
{
    public override string ErrorCode => "DATABASE_ERROR";
    
    public string Operation { get; }
    
    public string Entity { get; }

    public DatabaseOperationException(string message) : base(message) { }

    public DatabaseOperationException(string operation, string entity, string message) : base(message)
    {
        Operation = operation;
        Entity = entity;
    }

    public DatabaseOperationException(string message, Exception innerException) : base(message, innerException) { }

    public DatabaseOperationException(string operation, string entity, string message, Exception innerException) : base(message, innerException)
    {
        Operation = operation;
        Entity = entity;
    }
}