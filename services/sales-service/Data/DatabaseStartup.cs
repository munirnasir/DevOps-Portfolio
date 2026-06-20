using Microsoft.EntityFrameworkCore;

namespace Pos.Sales.Api.Data;

/// <summary>
/// Applies EF Core migrations on startup, retrying while the database container starts.
/// </summary>
public static class DatabaseStartup
{
    public static async Task MigrateAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SalesDbContext>>();

        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                logger.LogInformation("Sales database ready (migrated).");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{Max}); retrying in 3s...", attempt, maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }
}
