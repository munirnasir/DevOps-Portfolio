using Microsoft.EntityFrameworkCore;

namespace Pos.Catalog.Api.Data;

/// <summary>
/// Applies EF Core migrations and seeds demo data on startup. Retries because the
/// database container may still be coming up when the API process starts.
/// </summary>
public static class DatabaseStartup
{
    public static async Task MigrateAndSeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CatalogDbContext>>();

        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                await CatalogSeeder.SeedAsync(db);
                logger.LogInformation("Catalog database ready (migrated and seeded).");
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
