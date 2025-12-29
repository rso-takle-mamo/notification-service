namespace NotificationService.Api.Services.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string firstName, string lastName);
    // Task SendBookingConfirmationEmailAsync(string email, string firstName, string lastName, DateTime bookingDate);
}