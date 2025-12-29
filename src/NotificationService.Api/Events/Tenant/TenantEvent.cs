namespace NotificationService.Api.Events.Tenant;

public class TenantEvent : BaseEvent
{
    public Guid TenantId { get; set; }
    public Guid OwnerId { get; set; }
    public string VatNumber { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessEmail { get; set; } = string.Empty;
    public string? BusinessPhone { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}

public class TenantCreatedEvent : TenantEvent
{
    public TenantCreatedEvent()
    {
        EventType = nameof(TenantCreatedEvent);
    }
}

public class TenantUpdatedEvent : TenantEvent
{
    public TenantUpdatedEvent()
    {
        EventType = nameof(TenantUpdatedEvent);
    }
}