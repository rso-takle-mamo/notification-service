using NotificationService.Api.Events.Tenant;
using NotificationService.Api.Events.User;

namespace NotificationService.Api.Services.Interfaces;

public interface INotificationService
{
    // Customer Event Handlers
    Task HandleCustomerCreatedEventAsync(CustomerCreatedEvent customerEvent);
    Task HandleProviderCreatedEventAsync(ProviderCreatedEvent providerEvent);

    // User Event Handlers
    Task HandleUserUpdatedEventAsync(UserUpdatedEvent userEvent);
    Task HandleUserDeletedEventAsync(UserDeletedEvent userEvent);

    // Tenant Event Handlers
    Task HandleTenantCreatedEventAsync(TenantCreatedEvent tenantEvent);
    Task HandleTenantUpdatedEventAsync(TenantUpdatedEvent tenantEvent);

    // // Booking Handlers
    // Task HandleBookingCreatedEventAsync(Guid bookingId);
    // Task SendBookingConfirmationAsync(string email, string firstName, string lastName, DateTime bookingDate);
}