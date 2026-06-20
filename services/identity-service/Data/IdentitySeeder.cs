using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pos.Identity.Api.Domain;

namespace Pos.Identity.Api.Data;

/// <summary>
/// Seeds two demo accounts on first run so the POS is usable immediately.
/// Default credentials (override in any real deployment):
///   manager / manager123   (Manager)
///   cashier / cashier123    (Cashier)
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IdentityDbContext db, IPasswordHasher<User> hasher, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var manager = new User { Id = Guid.NewGuid(), Username = "manager", DisplayName = "Store Manager", Role = Roles.Manager, CreatedAtUtc = now };
        manager.PasswordHash = hasher.HashPassword(manager, "manager123");

        var cashier = new User { Id = Guid.NewGuid(), Username = "cashier", DisplayName = "Front Till", Role = Roles.Cashier, CreatedAtUtc = now };
        cashier.PasswordHash = hasher.HashPassword(cashier, "cashier123");

        db.Users.AddRange(manager, cashier);
        await db.SaveChangesAsync(ct);
    }
}
