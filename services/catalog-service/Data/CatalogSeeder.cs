using Microsoft.EntityFrameworkCore;
using Pos.Catalog.Api.Domain;

namespace Pos.Catalog.Api.Data;

/// <summary>
/// Seeds a small demo catalogue so the POS has something to sell on first run.
/// Idempotent: only inserts when the tables are empty.
/// </summary>
public static class CatalogSeeder
{
    public static async Task SeedAsync(CatalogDbContext db, CancellationToken ct = default)
    {
        if (await db.Categories.AnyAsync(ct))
        {
            return;
        }

        var beverages = new Category { Name = "Beverages", Description = "Drinks and juices" };
        var dairy = new Category { Name = "Dairy", Description = "Milk, cheese and eggs" };
        var bakery = new Category { Name = "Bakery", Description = "Bread and baked goods" };
        var snacks = new Category { Name = "Snacks", Description = "Chips, biscuits and confectionery" };

        db.Categories.AddRange(beverages, dairy, bakery, snacks);
        await db.SaveChangesAsync(ct);

        var now = DateTime.UtcNow;
        db.Products.AddRange(
            NewProduct("BEV-001", "5012345678900", "Cola 1.5L", beverages.Id, 1.20m, 0.15m, 120, now),
            NewProduct("BEV-002", "5012345678917", "Orange Juice 1L", beverages.Id, 1.80m, 0.15m, 80, now),
            NewProduct("BEV-003", "5012345678924", "Sparkling Water 500ml", beverages.Id, 0.65m, 0.15m, 200, now),
            NewProduct("DAI-001", "5012345678931", "Whole Milk 2L", dairy.Id, 1.45m, 0.00m, 60, now),
            NewProduct("DAI-002", "5012345678948", "Cheddar Cheese 400g", dairy.Id, 3.20m, 0.00m, 40, now),
            NewProduct("DAI-003", "5012345678955", "Free Range Eggs (12)", dairy.Id, 2.10m, 0.00m, 75, now),
            NewProduct("BAK-001", "5012345678962", "White Bread Loaf", bakery.Id, 0.95m, 0.00m, 90, now),
            NewProduct("BAK-002", "5012345678979", "Croissant 4pk", bakery.Id, 1.60m, 0.15m, 55, now),
            NewProduct("SNK-001", "5012345678986", "Salted Crisps 150g", snacks.Id, 1.10m, 0.15m, 150, now),
            NewProduct("SNK-002", "5012345678993", "Chocolate Bar 100g", snacks.Id, 0.85m, 0.15m, 220, now)
        );

        await db.SaveChangesAsync(ct);
    }

    private static Product NewProduct(
        string sku, string barcode, string name, int categoryId,
        decimal unitPrice, decimal taxRate, int stock, DateTime now) => new()
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            Barcode = barcode,
            Name = name,
            CategoryId = categoryId,
            UnitPrice = unitPrice,
            TaxRate = taxRate,
            StockQuantity = stock,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
}
