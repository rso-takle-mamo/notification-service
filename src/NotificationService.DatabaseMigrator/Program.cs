using Microsoft.EntityFrameworkCore;
using NotificationService.Database;

var dbContext = new NotificationDbContext();
var targetMigration = Environment.GetEnvironmentVariable("TARGET_MIGRATION");
var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
Console.WriteLine($"Applying pending migrations: [{string.Join(", ", pendingMigrations)}]");
await dbContext.Database.MigrateAsync(targetMigration);
Console.WriteLine("Finished applying pending migrations.");