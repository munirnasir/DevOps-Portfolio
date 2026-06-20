using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace Pos.Catalog.IntegrationTests;

/// <summary>
/// Boots the Catalog API against a real, throwaway PostgreSQL spun up by Testcontainers.
/// The app's startup migrates and seeds the container automatically.
/// </summary>
public class CatalogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("catalogdb")
        .WithUsername("pos")
        .WithPassword("pos")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:CatalogDb", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Key", TestAuth.Key);
        builder.UseSetting("Jwt:Issuer", TestAuth.Issuer);
        builder.UseSetting("Jwt:Audience", TestAuth.Audience);
    }
}
