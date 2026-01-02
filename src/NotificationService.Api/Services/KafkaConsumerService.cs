using System.Text.Json;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Api.Configuration;
using NotificationService.Api.Events;
using NotificationService.Api.Events.Tenant;
using NotificationService.Api.Events.User;
using NotificationService.Api.Events.Booking;
using NotificationService.Api.Services.Interfaces;

namespace NotificationService.Api.Services;

public class KafkaConsumerService(
    ILogger<KafkaConsumerService> logger,
    IServiceProvider serviceProvider,
    IOptions<KafkaSettings> kafkaSettings)
    : BackgroundService
{
    private readonly KafkaSettings _kafkaSettings = kafkaSettings.Value;

    private ConsumerConfig CreateUserConsumerConfig()
    {
        return new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = $"{_kafkaSettings.ConsumerGroupId}-user-events",
            AutoOffsetReset = KafkaConstants.ParseAutoOffsetReset(_kafkaSettings.AutoOffsetReset),
            EnableAutoCommit = _kafkaSettings.EnableAutoCommit
        };
    }

    private ConsumerConfig CreateTenantConsumerConfig()
    {
        return new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = $"{_kafkaSettings.ConsumerGroupId}-tenant-events",
            AutoOffsetReset = KafkaConstants.ParseAutoOffsetReset(_kafkaSettings.AutoOffsetReset),
            EnableAutoCommit = _kafkaSettings.EnableAutoCommit
        };
    }

    private ConsumerConfig CreateProviderConsumerConfig()
    {
        return new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = $"{_kafkaSettings.ConsumerGroupId}-provider-events",
            AutoOffsetReset = KafkaConstants.ParseAutoOffsetReset(_kafkaSettings.AutoOffsetReset),
            EnableAutoCommit = _kafkaSettings.EnableAutoCommit
        };
    }

    private ConsumerConfig CreateBookingConsumerConfig()
    {
        return new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = $"{_kafkaSettings.ConsumerGroupId}-booking-events",
            AutoOffsetReset = KafkaConstants.ParseAutoOffsetReset(_kafkaSettings.AutoOffsetReset),
            EnableAutoCommit = _kafkaSettings.EnableAutoCommit
        };
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka Consumer Service starting...");

        _ = Task.Run(async () =>
        {
            try
            {
                await ConsumeUserEventsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User events consumer failed unexpectedly.");
            }
        }, cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                await ConsumeTenantEventsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Tenant events consumer failed unexpectedly.");
            }
        }, cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                await ConsumeProviderEventsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Provider events consumer failed unexpectedly.");
            }
        }, cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                await ConsumeBookingEventsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Booking events consumer failed unexpectedly.");
            }
        }, cancellationToken);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task ConsumeUserEventsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting user events consumer for topic: {Topic}", _kafkaSettings.UserEventsTopic);

        var consumerConfig = CreateUserConsumerConfig();

        IConsumer<Ignore, string>? consumer = null;
        var retryCount = 0;
        var retryDelayMs = 2000;

        while (consumer == null && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
                consumer.Subscribe(_kafkaSettings.UserEventsTopic);
                logger.LogInformation("Successfully connected to Kafka and subscribed to topic: {Topic}", _kafkaSettings.UserEventsTopic);
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.LogWarning(ex, "Failed to initialize Kafka consumer (attempt {RetryCount}). Retrying in {Delay}ms...", retryCount, retryDelayMs);
                consumer?.Dispose();
                consumer = null;
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(retryDelayMs, cancellationToken);
                }
            }
        }

        if (consumer == null)
        {
            logger.LogError("Failed to initialize Kafka consumer after cancellation. Stopping consumer.");
            return;
        }

        try
        {
            using (consumer)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    if (consumeResult.Message is null) continue;

                    logger.LogInformation("Received user event: {EventType}", consumeResult.Message.Key);

                    try
                    {
                        var jsonNode = JsonNode.Parse(consumeResult.Message.Value);
                        var eventType = jsonNode?["eventType"]?.GetValue<string>();
                        if (string.IsNullOrEmpty(eventType))
                        {
                            logger.LogWarning("Event missing eventType field");
                            continue;
                        }

                        await ProcessUserEventAsync(eventType, consumeResult.Message.Value);
                        consumer.Commit(consumeResult);
                    }
                    catch (JsonException jsonEx)
                    {
                        logger.LogError(jsonEx, "Failed to parse user event JSON: {Message}", consumeResult.Message.Value);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing user event: {EventType}", consumeResult.Message.Key);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("User events consumer stopped due to cancellation");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "User events consumer error");
        }
    }

    private async Task ConsumeTenantEventsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting tenant events consumer for topic: {Topic}", _kafkaSettings.TenantEventsTopic);

        var consumerConfig = CreateTenantConsumerConfig();
        logger.LogInformation("Tenant consumer config created: BootstrapServers={BootstrapServers}, GroupId={GroupId}",
            consumerConfig.BootstrapServers, consumerConfig.GroupId);

        IConsumer<Ignore, string>? consumer = null;
        var retryCount = 0;
        var retryDelayMs = 2000;

        while (consumer == null && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Attempting to create tenant consumer (attempt {Attempt})", retryCount + 1);
                consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
                logger.LogInformation("Tenant consumer created successfully, subscribing to topic...");
                consumer.Subscribe(_kafkaSettings.TenantEventsTopic);
                logger.LogInformation("Successfully connected to Kafka and subscribed to topic: {Topic}", _kafkaSettings.TenantEventsTopic);
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.LogWarning(ex, "Failed to initialize Kafka consumer (attempt {RetryCount}). Retrying in {Delay}ms...", retryCount, retryDelayMs);
                consumer?.Dispose();
                consumer = null;
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(retryDelayMs, cancellationToken);
                }
            }
        }

        if (consumer == null)
        {
            logger.LogError("Failed to initialize Kafka consumer after cancellation. Stopping consumer.");
            return;
        }

        try
        {
            using (consumer)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    if (consumeResult.Message is null) continue;

                    logger.LogInformation("Received tenant event: {EventType}", consumeResult.Message.Key);

                    try
                    {
                        var jsonNode = JsonNode.Parse(consumeResult.Message.Value);
                        var eventType = jsonNode?["eventType"]?.GetValue<string>();
                        if (string.IsNullOrEmpty(eventType))
                        {
                            logger.LogWarning("Event missing eventType field");
                            continue;
                        }

                        await ProcessTenantEventAsync(eventType, consumeResult.Message.Value);
                        consumer.Commit(consumeResult);
                    }
                    catch (JsonException jsonEx)
                    {
                        logger.LogError(jsonEx, "Failed to parse tenant event JSON: {Message}", consumeResult.Message.Value);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing tenant event: {EventType}", consumeResult.Message.Key);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Tenant events consumer stopped due to cancellation");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Tenant events consumer error");
        }
    }

    private async Task ProcessUserEventAsync(string eventType, string eventJson)
    {
        using var scope = serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        try
        {
            switch (eventType)
            {
                case nameof(CustomerCreatedEvent):
                    var customerCreatedEvent = JsonSerializer.Deserialize<CustomerCreatedEvent>(eventJson, KafkaConstants.JsonSerializerOptions);
                    if (customerCreatedEvent != null)
                    {
                        logger.LogInformation("Processing CustomerCreatedEvent for user {UserId}", customerCreatedEvent.UserId);
                        await notificationService.HandleCustomerCreatedEventAsync(customerCreatedEvent);
                    }
                    break;

                case nameof(UserUpdatedEvent):
                    var userUpdatedEvent = JsonSerializer.Deserialize<UserUpdatedEvent>(eventJson, KafkaConstants.JsonSerializerOptions);
                    if (userUpdatedEvent != null)
                    {
                        logger.LogInformation("Processing UserUpdatedEvent for user {UserId}", userUpdatedEvent.UserId);
                        await notificationService.HandleUserUpdatedEventAsync(userUpdatedEvent);
                    }
                    break;

                case nameof(UserDeletedEvent):
                    var userDeletedEvent = JsonSerializer.Deserialize<UserDeletedEvent>(eventJson, KafkaConstants.JsonSerializerOptions);
                    if (userDeletedEvent != null)
                    {
                        logger.LogInformation("Processing UserDeletedEvent for user {UserId}", userDeletedEvent.UserId);
                        await notificationService.HandleUserDeletedEventAsync(userDeletedEvent);
                    }
                    break;

                default:
                    logger.LogWarning("Unknown user event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing user event {EventType}", eventType);
            throw;
        }
    }

    private async Task ProcessTenantEventAsync(string eventType, string eventJson)
    {
        using var scope = serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        try
        {
            switch (eventType)
            {
                case nameof(TenantCreatedEvent):
                    var tenantCreatedEvent = JsonSerializer.Deserialize<TenantCreatedEvent>(eventJson, KafkaConstants.JsonSerializerOptions);
                    if (tenantCreatedEvent != null)
                    {
                        logger.LogInformation("Processing TenantCreatedEvent for tenant {TenantId}", tenantCreatedEvent.TenantId);
                        await notificationService.HandleTenantCreatedEventAsync(tenantCreatedEvent);
                    }
                    break;

                case nameof(TenantUpdatedEvent):
                    var tenantUpdatedEvent = JsonSerializer.Deserialize<TenantUpdatedEvent>(eventJson, KafkaConstants.JsonSerializerOptions);
                    if (tenantUpdatedEvent != null)
                    {
                        logger.LogInformation("Processing TenantUpdatedEvent for tenant {TenantId}", tenantUpdatedEvent.TenantId);
                        await notificationService.HandleTenantUpdatedEventAsync(tenantUpdatedEvent);
                    }
                    break;

                default:
                    logger.LogWarning("Unknown tenant event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing tenant event {EventType}", eventType);
            throw;
        }
    }

    private async Task ConsumeProviderEventsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting provider events consumer for topic: {Topic}", _kafkaSettings.ProviderEventsTopic);

        var consumerConfig = CreateProviderConsumerConfig();

        IConsumer<Ignore, string>? consumer = null;
        var retryCount = 0;
        var retryDelayMs = 2000;

        while (consumer == null && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
                consumer.Subscribe(_kafkaSettings.ProviderEventsTopic);
                logger.LogInformation("Successfully connected to Kafka and subscribed to topic: {Topic}", _kafkaSettings.ProviderEventsTopic);
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.LogWarning(ex, "Failed to initialize Kafka consumer (attempt {RetryCount}). Retrying in {Delay}ms...", retryCount, retryDelayMs);
                consumer?.Dispose();
                consumer = null;
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(retryDelayMs, cancellationToken);
                }
            }
        }

        if (consumer == null)
        {
            logger.LogError("Failed to initialize Kafka consumer after cancellation. Stopping consumer.");
            return;
        }

        try
        {
            using (consumer)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    if (consumeResult.Message is null) continue;

                    logger.LogInformation("Received provider event: {EventType}", consumeResult.Message.Key);

                    try
                    {
                        var jsonNode = JsonNode.Parse(consumeResult.Message.Value);
                        var eventType = jsonNode?["eventType"]?.GetValue<string>();
                        if (string.IsNullOrEmpty(eventType))
                        {
                            logger.LogWarning("Event missing eventType field");
                            continue;
                        }

                        await ProcessProviderEventAsync(eventType, consumeResult.Message.Value);
                        consumer.Commit(consumeResult);
                    }
                    catch (JsonException jsonEx)
                    {
                        logger.LogError(jsonEx, "Failed to parse provider event JSON: {Message}", consumeResult.Message.Value);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing provider event: {EventType}", consumeResult.Message.Key);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Provider events consumer stopped due to cancellation");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Provider events consumer error");
        }
    }

    private async Task ProcessProviderEventAsync(string eventType, string eventJson)
    {
        using var scope = serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        try
        {
            switch (eventType)
            {
                case nameof(ProviderCreatedEvent):
                    var providerCreatedEvent = JsonSerializer.Deserialize<ProviderCreatedEvent>(eventJson, KafkaConstants.JsonSerializerOptions);
                    if (providerCreatedEvent != null)
                    {
                        logger.LogInformation("Processing ProviderCreatedEvent for provider {UserId}", providerCreatedEvent.UserId);
                        await notificationService.HandleProviderCreatedEventAsync(providerCreatedEvent);
                    }
                    break;

                default:
                    logger.LogWarning("Unknown provider event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing provider event {EventType}", eventType);
            throw;
        }
    }

    private async Task ConsumeBookingEventsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting booking events consumer for topic: {Topic}", _kafkaSettings.BookingEventsTopic);

        var consumerConfig = CreateBookingConsumerConfig();

        IConsumer<Ignore, string>? consumer = null;
        var retryCount = 0;
        var retryDelayMs = 2000;

        while (consumer == null && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
                consumer.Subscribe(_kafkaSettings.BookingEventsTopic);
                logger.LogInformation("Successfully connected to Kafka and subscribed to topic: {Topic}", _kafkaSettings.BookingEventsTopic);
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.LogWarning(ex, "Failed to initialize Kafka consumer (attempt {RetryCount}). Retrying in {Delay}ms...", retryCount, retryDelayMs);
                consumer?.Dispose();
                consumer = null;
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(retryDelayMs, cancellationToken);
                }
            }
        }

        if (consumer == null)
        {
            logger.LogError("Failed to initialize Kafka consumer after cancellation. Stopping consumer.");
            return;
        }

        try
        {
            using (consumer)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    if (consumeResult.Message is null) continue;

                    logger.LogInformation("Received booking event: {EventType}", consumeResult.Message.Key);

                    try
                    {
                        var jsonNode = JsonNode.Parse(consumeResult.Message.Value);
                        var eventType = jsonNode?["eventType"]?.GetValue<string>();
                        if (string.IsNullOrEmpty(eventType))
                        {
                            logger.LogWarning("Event missing eventType field");
                            continue;
                        }

                        await ProcessBookingEventAsync(eventType, consumeResult.Message.Value);
                        consumer.Commit(consumeResult);
                    }
                    catch (JsonException jsonEx)
                    {
                        logger.LogError(jsonEx, "Failed to parse booking event JSON: {Message}", consumeResult.Message.Value);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing booking event: {EventType}", consumeResult.Message.Key);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Booking events consumer stopped due to cancellation");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Booking events consumer error");
        }
    }

    private async Task ProcessBookingEventAsync(string eventType, string eventJson)
    {
        using var scope = serviceProvider.CreateScope();
        var bookingEventService = scope.ServiceProvider.GetRequiredService<IBookingEventService>();

        try
        {
            switch (eventType)
            {
                case nameof(BookingCreatedEvent):
                    var bookingCreatedEvent = JsonSerializer.Deserialize<BookingCreatedEvent>(eventJson, KafkaConstants.JsonSerializerOptions);
                    if (bookingCreatedEvent != null)
                    {
                        logger.LogInformation("Processing BookingCreatedEvent for booking {BookingId}", bookingCreatedEvent.BookingId);
                        await bookingEventService.HandleBookingCreatedEventAsync(bookingCreatedEvent);
                    }
                    break;

                case nameof(BookingCancelledEvent):
                    var bookingCancelledEvent = JsonSerializer.Deserialize<BookingCancelledEvent>(eventJson, KafkaConstants.JsonSerializerOptions);
                    if (bookingCancelledEvent != null)
                    {
                        logger.LogInformation("Processing BookingCancelledEvent for booking {BookingId}", bookingCancelledEvent.BookingId);
                        await bookingEventService.HandleBookingCancelledEventAsync(bookingCancelledEvent);
                    }
                    break;

                default:
                    logger.LogWarning("Unknown booking event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing booking event {EventType}", eventType);
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka Consumer Service stopping...");
        await base.StopAsync(cancellationToken);
    }
}