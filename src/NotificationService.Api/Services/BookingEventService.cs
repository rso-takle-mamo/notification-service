using Microsoft.Extensions.Logging;
using NotificationService.Api.Events.Booking;
using NotificationService.Api.Services.Interfaces;
using NotificationService.Database.Repositories.Interfaces;

namespace NotificationService.Api.Services;

public class BookingEventService(
    ILogger<BookingEventService> logger,
    IBookingRepository bookingRepository,
    IUserRepository userRepository) : IBookingEventService
{
    public async Task HandleBookingCreatedEventAsync(BookingCreatedEvent bookingEvent)
    {
        logger.LogInformation("Handling booking created event for booking ID: {BookingId}", bookingEvent.BookingId);

        try
        {
            var existingBooking = await bookingRepository.GetByIdAsync(bookingEvent.BookingId);
            if (existingBooking != null)
            {
                logger.LogWarning("Booking with ID {BookingId} already exists, skipping creation", bookingEvent.BookingId);
                return;
            }

            var booking = new Database.Entities.Booking
            {
                Id = bookingEvent.BookingId,
                TenantId = bookingEvent.TenantId,
                StartDateTime = bookingEvent.StartDateTime,
                EndDateTime = bookingEvent.EndDateTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await bookingRepository.CreateAsync(booking);
            logger.LogInformation("Successfully created booking {BookingId} in notification database", bookingEvent.BookingId);

            // Mock sending email notification
            await MockSendBookingCreatedEmailAsync(bookingEvent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling booking created event for booking ID: {BookingId}", bookingEvent.BookingId);
            throw;
        }
    }

    public async Task HandleBookingCancelledEventAsync(BookingCancelledEvent bookingEvent)
    {
        logger.LogInformation("Handling booking cancelled event for booking ID: {BookingId}", bookingEvent.BookingId);

        try
        {
            var existingBooking = await bookingRepository.GetByIdAsync(bookingEvent.BookingId);
            if (existingBooking == null)
            {
                logger.LogWarning("Booking with ID {BookingId} not found for cancellation", bookingEvent.BookingId);
                return;
            }

            await bookingRepository.DeleteAsync(existingBooking);
            logger.LogInformation("Successfully deleted booking {BookingId} from notification database", bookingEvent.BookingId);

            // Mock sending email notification
            await MockSendBookingCancelledEmailAsync(bookingEvent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling booking cancelled event for booking ID: {BookingId}", bookingEvent.BookingId);
            throw;
        }
    }

    private async Task MockSendBookingCreatedEmailAsync(BookingCreatedEvent bookingEvent)
    {
        // Mock email sending - in production, this would call an actual email service
        var user = await userRepository.GetByIdAsync(bookingEvent.OwnerId);
        var userName = user?.FirstName ?? "User";

        logger.LogInformation("""
            [MOCK EMAIL] Booking Created Notification
            To: {UserName} <{UserId}>
            Subject: Your booking has been confirmed!
            Body:
            Dear {UserName},

            Your booking has been successfully created!
            Booking ID: {BookingId}
            Service ID: {ServiceId}
            Start Time: {StartDateTime:yyyy-MM-dd HH:mm}
            End Time: {EndDateTime:yyyy-MM-dd HH:mm}
            Notes: {Notes}

            Thank you for your booking!
            """,
            userName,
            bookingEvent.OwnerId,
            userName,
            bookingEvent.BookingId,
            bookingEvent.ServiceId,
            bookingEvent.StartDateTime,
            bookingEvent.EndDateTime,
            bookingEvent.Notes ?? "N/A");

        await Task.CompletedTask;
    }

    private async Task MockSendBookingCancelledEmailAsync(BookingCancelledEvent bookingEvent)
    {
        // Mock email sending - in production, this would call an actual email service
        var user = await userRepository.GetByIdAsync(bookingEvent.OwnerId);
        var userName = user?.FirstName ?? "User";

        logger.LogInformation("""
            [MOCK EMAIL] Booking Cancelled Notification
            To: {UserName} <{UserId}>
            Subject: Your booking has been cancelled
            Body:
            Dear {UserName},

            Your booking has been cancelled as requested.
            Booking ID: {BookingId}
            Service ID: {ServiceId}

            We hope to serve you again soon!
            """,
            userName,
            bookingEvent.OwnerId,
            userName,
            bookingEvent.BookingId,
            bookingEvent.ServiceId);

        await Task.CompletedTask;
    }
}
