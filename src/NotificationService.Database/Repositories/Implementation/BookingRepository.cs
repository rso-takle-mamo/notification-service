using Microsoft.EntityFrameworkCore;
using NotificationService.Database;
using NotificationService.Database.Entities;
using NotificationService.Database.Repositories.Interfaces;

namespace NotificationService.Database.Repositories.Implementation;

public class BookingRepository(NotificationDbContext context) : IBookingRepository
{
    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await context.Bookings
            .Include(b => b.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Booking>> GetByTenantIdAsync(Guid tenantId)
    {
        return await context.Bookings
            .Include(b => b.Tenant)
            .AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetBookingsByDateRangeAsync(Guid tenantId, DateTime startDate, DateTime endDate)
    {
        return await context.Bookings
            .Include(b => b.Tenant)
            .AsNoTracking()
            .Where(b => b.TenantId == tenantId &&
                       b.StartDateTime >= startDate &&
                       b.EndDateTime <= endDate)
            .ToListAsync();
    }

    public async Task<Booking> CreateAsync(Booking booking)
    {
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking> UpdateAsync(Booking booking)
    {
        context.Bookings.Update(booking);
        await context.SaveChangesAsync();
        return booking;
    }

    public async Task DeleteAsync(Booking booking)
    {
        context.Bookings.Remove(booking);
        await context.SaveChangesAsync();
    }
}