using Microsoft.Extensions.Logging;
using NotificationService.Api.Events.Booking;
using NotificationService.Api.Services.Interfaces;
using NotificationService.Database.Repositories.Interfaces;

namespace NotificationService.Api.Services;

public class BookingEventService(
    ILogger<BookingEventService> logger,
    IBookingRepository bookingRepository,
    IUserRepository userRepository,
    IEmailService emailService) : IBookingEventService
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

            // Send email notification
            var user = await userRepository.GetByIdAsync(bookingEvent.OwnerId);
            if (user != null)
            {
                var userName = $"{user.FirstName} {user.LastName}".Trim();
                await emailService.SendBookingCreatedEmailAsync(
                    user.Email,
                    user.FirstName,
                    userName,
                    bookingEvent.BookingId,
                    bookingEvent.ServiceId,
                    bookingEvent.StartDateTime,
                    bookingEvent.EndDateTime,
                    bookingEvent.Notes);
            }
            else
            {
                logger.LogWarning("User with ID {UserId} not found, skipping email notification", bookingEvent.OwnerId);
            }
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

            // Send email notification
            var user = await userRepository.GetByIdAsync(bookingEvent.OwnerId);
            if (user != null)
            {
                var userName = $"{user.FirstName} {user.LastName}".Trim();
                await emailService.SendBookingCancelledEmailAsync(
                    user.Email,
                    user.FirstName,
                    userName,
                    bookingEvent.BookingId,
                    bookingEvent.ServiceId);
            }
            else
            {
                logger.LogWarning("User with ID {UserId} not found, skipping email notification", bookingEvent.OwnerId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling booking cancelled event for booking ID: {BookingId}", bookingEvent.BookingId);
            throw;
        }
    }
}
