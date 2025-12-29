using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Database.Repositories.Implementation;
using NotificationService.Database.Repositories.Interfaces;

namespace NotificationService.Database;

public static class NotificationDatabaseExtensions
{
    public static void AddNotificationDatabase(this IServiceCollection services)
    {
        services.AddDbContext<NotificationDbContext>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
    }
}