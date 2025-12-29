using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NotificationService.Api.Configuration;

namespace NotificationService.Api.HealthChecks;

public class KafkaHealthCheck(
    IOptions<KafkaSettings> kafkaSettings,
    ILogger<KafkaHealthCheck> logger)
    : IHealthCheck
{
    private readonly KafkaSettings _kafkaSettings = kafkaSettings.Value;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                MessageTimeoutMs = 1000
            };

            using var producer = new ProducerBuilder<Null, string>(config).Build();

            // If we can create a producer without exception, Kafka is reachable
            return Task.FromResult(HealthCheckResult.Healthy("Kafka is reachable"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kafka health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Kafka is not reachable", ex));
        }
    }
}