using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pos.Sales.Api.Services;
using Testcontainers.PostgreSql;
using Xunit;

namespace Pos.Sales.IntegrationTests;

/// <summary>
/// Boots the Sales API against a throwaway PostgreSQL, with the Catalog HTTP client
/// swapped for <see cref="FakeCatalogClient"/> so checkout can be tested in isolation.
/// </summary>
public class SalesApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public FakeCatalogClient Catalog { get; } = new();

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("salesdb")
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
        builder.UseSetting("ConnectionStrings:SalesDb", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Key", TestAuth.Key);
        builder.UseSetting("Jwt:Issuer", TestAuth.Issuer);
        builder.UseSetting("Jwt:Audience", TestAuth.Audience);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICatalogClient>();
            services.AddSingleton<ICatalogClient>(Catalog);
        });
    }
}
