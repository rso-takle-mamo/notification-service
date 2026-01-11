namespace NotificationService.Api.Configuration;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string UserEventsTopic { get; set; } = string.Empty;
    public string TenantEventsTopic { get; set; } = string.Empty;
    public string ProviderEventsTopic { get; set; } = string.Empty;
    public string BookingEventsTopic { get; set; } = string.Empty;
    public string ConsumerGroupId { get; set; } = string.Empty;
    public bool EnableAutoCommit { get; set; }
    public string AutoOffsetReset { get; set; } = "Earliest";

    public string SecurityProtocol { get; set; } = "SaslSsl";
    public string SaslMechanism { get; set; } = "Plain";
    public string SaslUsername { get; set; } = "$ConnectionString";
    public string SaslPassword { get; set; } = string.Empty;
}
