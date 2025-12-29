using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;

namespace NotificationService.Api.Configuration;

public static class KafkaConstants
{
    /// <summary>
    /// Standard JSON serialization options for Kafka events.
    /// Uses camelCase property naming and omits null values.
    /// </summary>
    public static JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Converts string AutoOffsetReset value to Confluent.Kafka.AutoOffsetReset enum.
    /// </summary>
    public static Confluent.Kafka.AutoOffsetReset ParseAutoOffsetReset(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "earliest" => Confluent.Kafka.AutoOffsetReset.Earliest,
            "latest" => Confluent.Kafka.AutoOffsetReset.Latest,
            "none" or "error" => Confluent.Kafka.AutoOffsetReset.Error,
            _ => Confluent.Kafka.AutoOffsetReset.Earliest // Default
        };
    }
}
