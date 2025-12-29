using Microsoft.EntityFrameworkCore;
using NotificationService.Database;
using NotificationService.Database.Entities;
using NotificationService.Database.Repositories.Interfaces;

namespace NotificationService.Database.Repositories.Implementation;

public class TenantRepository(NotificationDbContext context) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        return await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Tenant>> GetAllAsync()
    {
        return await context.Tenants
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Tenant?> GetByBusinessEmailAsync(string email)
    {
        return await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.BusinessEmail == email);
    }

    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        await context.Tenants.AddAsync(tenant);
        await context.SaveChangesAsync();
        return tenant;
    }

    public async Task<Tenant> UpdateAsync(Tenant tenant)
    {
        context.Tenants.Update(tenant);
        await context.SaveChangesAsync();
        return tenant;
    }

    public async Task DeleteAsync(Tenant tenant)
    {
        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync();
    }
}