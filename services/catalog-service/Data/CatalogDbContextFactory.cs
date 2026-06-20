using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pos.Catalog.Api.Data;

/// <summary>
/// Lets the EF Core CLI (migrations) construct the context without booting the web host,
/// so adding a migration never touches the runtime startup/seed path.
/// </summary>
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=catalogdb;Username=pos;Password=pos")
            .Options;

        return new CatalogDbContext(options);
    }
}
