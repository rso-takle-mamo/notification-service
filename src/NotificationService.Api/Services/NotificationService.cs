using Microsoft.Extensions.Logging;
using NotificationService.Api.Events;
using NotificationService.Api.Events.Tenant;
using NotificationService.Api.Events.User;
using NotificationService.Api.Services.Interfaces;
using NotificationService.Database.Entities;
using NotificationService.Database.Repositories.Interfaces;

namespace NotificationService.Api.Services;

public class NotificationService(
    ILogger<NotificationService> logger,
    IEmailService emailService,
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IBookingRepository bookingRepository) : INotificationService
{
    // User Event Handlers
    public async Task HandleCustomerCreatedEventAsync(CustomerCreatedEvent customerEvent)
    {
        logger.LogInformation("Handling customer created event for user ID: {UserId}", customerEvent.UserId);

        try
        {
            // Check if user already exists to prevent duplicates
            var existingUser = await userRepository.GetByIdAsync(customerEvent.UserId);
            if (existingUser != null)
            {
                logger.LogWarning("User with ID {UserId} already exists, skipping creation", customerEvent.UserId);
                return;
            }

            var user = new User
            {
                Id = customerEvent.UserId,
                FirstName = customerEvent.FirstName,
                LastName = customerEvent.LastName,
                Email = customerEvent.Email,
                TenantId = customerEvent.TenantId
            };

            await userRepository.CreateAsync(user);
            logger.LogInformation("Successfully created customer {UserId} in notification database", customerEvent.UserId);

            // Send welcome email
            await emailService.SendWelcomeEmailAsync(
                user.Email,
                user.FirstName,
                user.LastName);

            logger.LogInformation("Successfully sent welcome email to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling customer created event for user ID: {UserId}", customerEvent.UserId);
            throw;
        }
    }

    public async Task HandleProviderCreatedEventAsync(ProviderCreatedEvent providerEvent)
    {
        logger.LogInformation("Handling provider created event for provider ID: {UserId}", providerEvent.UserId);

        try
        {
            // First, create the tenant if it doesn't exist
            var existingTenant = await tenantRepository.GetByIdAsync(providerEvent.TenantId);
            if (existingTenant == null)
            {
                var tenant = new Tenant
                {
                    Id = providerEvent.TenantId,
                    OwnerId = providerEvent.OwnerId,
                    VatNumber = providerEvent.VatNumber,
                    BusinessName = providerEvent.BusinessName,
                    BusinessEmail = providerEvent.BusinessEmail,
                    BusinessPhone = providerEvent.BusinessPhone,
                    Address = providerEvent.Address ?? string.Empty,
                    Description = providerEvent.Description
                };

                await tenantRepository.CreateAsync(tenant);
                logger.LogInformation("Successfully created tenant {TenantId} in notification database", providerEvent.TenantId);
            }
            else
            {
                logger.LogInformation("Tenant {TenantId} already exists, skipping creation", providerEvent.TenantId);
            }

            // Then, create the user if it doesn't exist
            var existingUser = await userRepository.GetByIdAsync(providerEvent.UserId);
            if (existingUser == null)
            {
                var user = new User
                {
                    Id = providerEvent.UserId,
                    FirstName = providerEvent.FirstName,
                    LastName = providerEvent.LastName,
                    Email = providerEvent.Email,
                    TenantId = providerEvent.TenantId
                };

                await userRepository.CreateAsync(user);
                logger.LogInformation("Successfully created provider user {UserId} in notification database", providerEvent.UserId);

                // Send welcome email
                await emailService.SendWelcomeEmailAsync(
                    user.Email,
                    user.FirstName,
                    user.LastName);

                logger.LogInformation("Successfully sent welcome email to {Email}", user.Email);
            }
            else
            {
                logger.LogInformation("User {UserId} already exists, skipping creation", providerEvent.UserId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling provider created event for provider ID: {UserId}", providerEvent.UserId);
            throw;
        }
    }

    public async Task HandleUserUpdatedEventAsync(UserUpdatedEvent userEvent)
    {
        logger.LogInformation("Handling user updated event for user ID: {UserId}", userEvent.UserId);

        try
        {
            // Create a new user entity with updated properties
            // Since GetByIdAsync uses AsNoTracking, we need to create a new entity for update
            var updatedUser = new User
            {
                Id = userEvent.UserId,
                FirstName = userEvent.FirstName,
                LastName = userEvent.LastName,
                Email = userEvent.Email,
                TenantId = userEvent.TenantId
                // CreatedAt is preserved from existing record
                // UpdatedAt is set automatically by the database context on update
            };

            await userRepository.UpdateAsync(updatedUser);
            logger.LogInformation("Successfully updated user {UserId} in notification database", userEvent.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling user updated event for user ID: {UserId}", userEvent.UserId);
            throw;
        }
    }

    public async Task HandleUserDeletedEventAsync(UserDeletedEvent userEvent)
    {
        logger.LogInformation("Handling user deleted event for user ID: {UserId}", userEvent.UserId);

        try
        {
            var existingUser = await userRepository.GetByIdAsync(userEvent.UserId);
            if (existingUser == null)
            {
                logger.LogWarning("User with ID {UserId} not found for deletion", userEvent.UserId);
                return;
            }

            await userRepository.DeleteAsync(existingUser);
            logger.LogInformation("Successfully deleted user {UserId} from notification database", userEvent.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling user deleted event for user ID: {UserId}", userEvent.UserId);
            throw;
        }
    }

    // Tenant Event Handlers
    public async Task HandleTenantCreatedEventAsync(TenantCreatedEvent tenantEvent)
    {
        logger.LogInformation("Handling tenant created event for tenant ID: {TenantId}", tenantEvent.TenantId);

        try
        {
            // Check if tenant already exists to prevent duplicates
            var existingTenant = await tenantRepository.GetByIdAsync(tenantEvent.TenantId);
            if (existingTenant != null)
            {
                logger.LogWarning("Tenant with ID {TenantId} already exists, skipping creation", tenantEvent.TenantId);
                return;
            }

            // Create tenant in notification database
            var tenant = new Tenant
            {
                Id = tenantEvent.TenantId,
                OwnerId = tenantEvent.OwnerId,
                VatNumber = tenantEvent.VatNumber,
                BusinessName = tenantEvent.BusinessName,
                BusinessEmail = tenantEvent.BusinessEmail,
                BusinessPhone = tenantEvent.BusinessPhone,
                Address = tenantEvent.Address ?? string.Empty, // Handle null address
                Description = tenantEvent.Description
                // CreatedAt and UpdatedAt are set automatically by the database context
            };

            await tenantRepository.CreateAsync(tenant);
            logger.LogInformation("Successfully created tenant {TenantId} in notification database", tenantEvent.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling tenant created event for tenant ID: {TenantId}", tenantEvent.TenantId);
            throw;
        }
    }

    public async Task HandleTenantUpdatedEventAsync(TenantUpdatedEvent tenantEvent)
    {
        logger.LogInformation("Handling tenant updated event for tenant ID: {TenantId}", tenantEvent.TenantId);

        try
        {
            // Create a new tenant entity with updated properties
            // Since GetByIdAsync uses AsNoTracking, we need to create a new entity for update
            var updatedTenant = new Tenant
            {
                Id = tenantEvent.TenantId,
                OwnerId = tenantEvent.OwnerId,
                VatNumber = tenantEvent.VatNumber,
                BusinessName = tenantEvent.BusinessName,
                BusinessEmail = tenantEvent.BusinessEmail,
                BusinessPhone = tenantEvent.BusinessPhone,
                Address = tenantEvent.Address ?? string.Empty, // Handle null address
                Description = tenantEvent.Description
                // CreatedAt is preserved from existing record
                // UpdatedAt is set automatically by the database context on update
            };

            await tenantRepository.UpdateAsync(updatedTenant);
            logger.LogInformation("Successfully updated tenant {TenantId} in notification database", tenantEvent.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling tenant updated event for tenant ID: {TenantId}", tenantEvent.TenantId);
            throw;
        }
    }
    

    // public async Task HandleBookingCreatedEventAsync(Guid bookingId)
    // {
    //     logger.LogInformation("Handling booking created event for booking ID: {BookingId}", bookingId);
    //
    //     try
    //     {
    //         var booking = await bookingRepository.GetByIdAsync(bookingId);
    //         if (booking == null)
    //         {
    //             logger.LogWarning("Booking with ID {BookingId} not found", bookingId);
    //             return;
    //         }
    //
    //         // For now, we'll assume the booking has a user ID
    //         // In a real implementation, you'd have the user ID in the booking or related data
    //         logger.LogInformation("Booking created for Tenant: {TenantId} on {Date}",
    //             booking.TenantId, booking.StartDateTime);
    //
    //         // This is a placeholder - in a real implementation, you'd get the user email from the booking
    //         // or from the event data when Kafka is integrated
    //         logger.LogInformation("Booking created notification sent");
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Error processing booking creation notification for booking ID: {BookingId}", bookingId);
    //         throw;
    //     }
    // }
    //
    // public async Task SendBookingConfirmationAsync(string email, string firstName, string lastName, DateTime bookingDate)
    // {
    //     logger.LogInformation("Sending booking confirmation to {Email} for {Date}", email, bookingDate);
    //
    //     try
    //     {
    //         await emailService.SendBookingConfirmationEmailAsync(email, firstName, lastName, bookingDate);
    //         logger.LogInformation("Booking confirmation sent successfully to {Email}", email);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Error sending booking confirmation to {Email}", email);
    //         throw;
    //     }
    // }
}