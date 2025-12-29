using Microsoft.EntityFrameworkCore;
using NotificationService.Database;
using NotificationService.Database.Entities;
using NotificationService.Database.Repositories.Interfaces;

namespace NotificationService.Database.Repositories.Implementation;

public class UserRepository(NotificationDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetByTenantIdAsync(Guid tenantId)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(User user)
    {
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}