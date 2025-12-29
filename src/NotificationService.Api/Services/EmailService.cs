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

//     public Task SendBookingConfirmationEmailAsync(string email, string firstName, string lastName, DateTime bookingDate)
//     {
//         logger.LogInformation(
//             "[MOCK] Sending booking confirmation email to {Email} for {FirstName} {LastName} on {BookingDate}",
//             email, firstName, lastName, bookingDate);
//
//         var emailContent = $@"
//         Subject: Booking Confirmation
//
//         Dear {firstName} {lastName},
//
//         Your booking has been confirmed for {bookingDate:yyyy-MM-dd HH:mm}.
//
//         Please arrive 5 minutes before your scheduled time.
//
//         Best regards,
//         The Team
//         ";
//
//         logger.LogDebug("Email content: {Content}", emailContent);
//
//         Task.Delay(150);
//
//         logger.LogInformation("Booking confirmation email sent to {Email}", email);
//         return Task.CompletedTask;
//     }
}