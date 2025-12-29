using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using NotificationService.Api.Configuration;
using NotificationService.Api.HealthChecks;
using NotificationService.Api.Middleware;
using NotificationService.Api.Services;
using NotificationService.Api.Services.Interfaces;
using NotificationService.Database;
using Prometheus;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Configure Kafka settings
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddNotificationDatabase();

// Register application services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService.Api.Services.NotificationService>();

// Register Kafka consumer as background service
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddHealthChecks()
    .AddCheck("self", () =>
    {
        try
        {
            return HealthCheckResult.Healthy("NotificationService is running");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Service check failed", ex);
        }
    }, tags: ["self"])
    .AddNpgSql(
        connectionString: EnvironmentVariables.GetRequiredVariable("DATABASE_CONNECTION_STRING"),
        healthQuery: "SELECT 1;",
        name: "postgresql",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "postgresql"])
    .AddCheck<KafkaHealthCheck>("kafka", tags: ["kafka", "messaging"]);

// Register middleware
builder.Services.AddTransient<GlobalExceptionHandler>();


var app = builder.Build();

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandler>();

app.UseHttpsRedirection();

// Add /metrics endpoints for Prometheus
app.UseHttpMetrics();
app.MapMetrics();


// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("self"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("self") || check.Tags.Contains("db"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

app.Run();