using NotificationService.Api.Services.Interfaces;

namespace NotificationService.Api.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendWelcomeEmailAsync(string email, string firstName, string lastName)
    {
        logger.LogInformation(
            "[MOCK] Sending welcome email to {Email} for user {FirstName} {LastName}",
            email, firstName, lastName);

        // Mock email content
        var emailContent = $@"
        Subject: Welcome to Our Service!

        Dear {firstName} {lastName},

        Welcome to our appointment booking system! We're excited to have you on board.

        Best regards,
        The Team
        ";

        logger.LogDebug("Email content: {Content}", emailContent);

        // Simulate email sending delay
        Task.Delay(100);

        logger.LogInformation("Welcome email successfully sent to {Email}", email);
        return Task.CompletedTask;
    }

    public Task SendBookingCreatedEmailAsync(string email, string firstName, string userName, Guid bookingId, Guid serviceId, DateTime startDateTime, DateTime endDateTime, string? notes)
    {
        logger.LogInformation(
            "[MOCK] Sending booking created email to {Email} for user {UserName}",
            email, userName);

        // Mock email content
        var emailContent = $@"
        Subject: Your booking has been confirmed!

        Dear {userName},

        Your booking has been successfully created!

        Booking ID: {bookingId}
        Service ID: {serviceId}
        Start Time: {startDateTime:yyyy-MM-dd HH:mm}
        End Time: {endDateTime:yyyy-MM-dd HH:mm}
        Notes: {notes ?? "N/A"}

        Thank you for your booking!

        Best regards,
        The Team
        ";

        logger.LogDebug("Email content: {Content}", emailContent);

        // Simulate email sending delay
        Task.Delay(150);

        logger.LogInformation("Booking created email successfully sent to {Email}", email);
        return Task.CompletedTask;
    }

    public Task SendBookingCancelledEmailAsync(string email, string firstName, string userName, Guid bookingId, Guid serviceId)
    {
        logger.LogInformation(
            "[MOCK] Sending booking cancelled email to {Email} for user {UserName}",
            email, userName);

        // Mock email content
        var emailContent = $@"
        Subject: Your booking has been cancelled

        Dear {userName},

        Your booking has been cancelled as requested.

        Booking ID: {bookingId}
        Service ID: {serviceId}

        We hope to serve you again soon!

        Best regards,
        The Team
        ";

        logger.LogDebug("Email content: {Content}", emailContent);

        // Simulate email sending delay
        Task.Delay(150);

        logger.LogInformation("Booking cancelled email successfully sent to {Email}", email);
        return Task.CompletedTask;
    }
}