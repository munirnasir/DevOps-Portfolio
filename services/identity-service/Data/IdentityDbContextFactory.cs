using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pos.Identity.Api.Data;

/// <summary>Lets the EF Core CLI build the context without booting the web host.</summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=identitydb;Username=pos;Password=pos")
            .Options;

        return new IdentityDbContext(options);
    }
}
