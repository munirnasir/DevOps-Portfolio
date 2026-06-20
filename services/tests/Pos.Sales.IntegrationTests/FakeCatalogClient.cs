using System.Collections.Concurrent;
using Pos.Sales.Api.Services;

namespace Pos.Sales.IntegrationTests;

/// <summary>
/// Stands in for the real Catalog HTTP client so Sales checkout can be tested without a
/// running Catalog service. Returns a fixed-price product and records stock adjustments.
/// </summary>
public class FakeCatalogClient : ICatalogClient
{
    public decimal UnitPrice { get; set; } = 2.00m;
    public decimal TaxRate { get; set; } = 0.10m;
    public int StockOnHand { get; set; } = 100;

    public ConcurrentBag<(Guid ProductId, int Change)> Adjustments { get; } = new();

    public Task<CatalogProduct?> GetProductAsync(Guid productId, CancellationToken ct = default)
    {
        var product = new CatalogProduct(productId, "FAKE-SKU", "Fake Product", UnitPrice, TaxRate, StockOnHand, IsActive: true);
        return Task.FromResult<CatalogProduct?>(product);
    }

    public Task<bool> AdjustStockAsync(Guid productId, int quantityChange, string reason, CancellationToken ct = default)
    {
        Adjustments.Add((productId, quantityChange));
        return Task.FromResult(true);
    }
}
