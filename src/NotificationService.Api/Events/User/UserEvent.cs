namespace NotificationService.Api.Events.User;

public class UserEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
    public Guid? TenantId { get; set; }
}

public class CustomerCreatedEvent : UserEvent
{
    public CustomerCreatedEvent()
    {
        EventType = nameof(CustomerCreatedEvent);
    }
}

public class UserUpdatedEvent : UserEvent
{
    public UserUpdatedEvent()
    {
        EventType = nameof(UserUpdatedEvent);
    }
}

public class UserDeletedEvent : BaseEvent
{
    public UserDeletedEvent()
    {
        EventType = nameof(UserDeletedEvent);
    }

    public Guid UserId { get; set; }
}

public class ProviderCreatedEvent : BaseEvent
{
    public ProviderCreatedEvent()
    {
        EventType = nameof(ProviderCreatedEvent);
    }

    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }

    public Guid TenantId { get; set; }
    public Guid OwnerId { get; set; }
    public string VatNumber { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessEmail { get; set; }
    public string? BusinessPhone { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
}