using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pos.Sales.Api.Data;

/// <summary>
/// Lets the EF Core CLI (migrations) construct the context without booting the web host.
/// </summary>
public class SalesDbContextFactory : IDesignTimeDbContextFactory<SalesDbContext>
{
    public SalesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=salesdb;Username=pos;Password=pos")
            .Options;

        return new SalesDbContext(options);
    }
}
