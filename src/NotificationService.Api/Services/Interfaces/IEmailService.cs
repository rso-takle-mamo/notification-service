namespace NotificationService.Api.Services.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string firstName, string lastName);
    Task SendBookingCreatedEmailAsync(string email, string firstName, string userName, Guid bookingId, Guid serviceId, DateTime startDateTime, DateTime endDateTime, string? notes);
    Task SendBookingCancelledEmailAsync(string email, string firstName, string userName, Guid bookingId, Guid serviceId);
}