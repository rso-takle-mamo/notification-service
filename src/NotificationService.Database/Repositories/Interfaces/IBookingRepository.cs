using NotificationService.Database.Entities;

namespace NotificationService.Database.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id);
    Task<List<Booking>> GetByTenantIdAsync(Guid tenantId);
    Task<List<Booking>> GetBookingsByDateRangeAsync(Guid tenantId, DateTime startDate, DateTime endDate);
    Task<Booking> CreateAsync(Booking booking);
    Task<Booking> UpdateAsync(Booking booking);
    Task DeleteAsync(Booking booking);
}