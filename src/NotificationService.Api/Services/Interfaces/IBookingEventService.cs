using NotificationService.Api.Events.Booking;

namespace NotificationService.Api.Services.Interfaces;

public interface IBookingEventService
{
    Task HandleBookingCreatedEventAsync(BookingCreatedEvent bookingEvent);
    Task HandleBookingCancelledEventAsync(BookingCancelledEvent bookingEvent);
}
