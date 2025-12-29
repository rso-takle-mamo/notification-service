namespace NotificationService.Api.Models;

public class Error
{
    public required string Code { get; set; }
    public required string Message { get; set; }
    public string? ResourceType { get; set; }
    public object? ResourceId { get; set; }
}