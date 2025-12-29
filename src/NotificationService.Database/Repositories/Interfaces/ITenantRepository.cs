using NotificationService.Database.Entities;

namespace NotificationService.Database.Repositories.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<List<Tenant>> GetAllAsync();
    Task<Tenant?> GetByBusinessEmailAsync(string email);
    Task<Tenant> CreateAsync(Tenant tenant);
    Task<Tenant> UpdateAsync(Tenant tenant);
    Task DeleteAsync(Tenant tenant);
}