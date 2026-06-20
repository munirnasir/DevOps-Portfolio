using System.Net;

namespace Pos.Sales.Api.Services;

/// <summary>Subset of the Catalog product contract that the Sales service depends on.</summary>
public record CatalogProduct(
    Guid Id,
    string Sku,
    string Name,
    decimal UnitPrice,
    decimal TaxRate,
    int StockQuantity,
    bool IsActive);

public record StockAdjustment(int QuantityChange, string? Reason);

public interface ICatalogClient
{
    Task<CatalogProduct?> GetProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Adjusts stock in the Catalog. Returns false when the Catalog rejects it (e.g. insufficient stock).</summary>
    Task<bool> AdjustStockAsync(Guid productId, int quantityChange, string reason, CancellationToken ct = default);
}

/// <summary>
/// Typed HttpClient wrapper around the Catalog service. Base address and timeout are
/// configured in <c>Program.cs</c> so the dependency is explicit and testable.
/// </summary>
public class CatalogClient : ICatalogClient
{
    private readonly HttpClient _http;
    private readonly ILogger<CatalogClient> _logger;

    public CatalogClient(HttpClient http, ILogger<CatalogClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<CatalogProduct?> GetProductAsync(Guid productId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/products/{productId}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CatalogProduct>(cancellationToken: ct);
    }

    public async Task<bool> AdjustStockAsync(Guid productId, int quantityChange, string reason, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/products/{productId}/stock-adjustment",
            new StockAdjustment(quantityChange, reason),
            ct);

        if (response.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Catalog rejected stock adjustment for {ProductId}: {Status}", productId, response.StatusCode);
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }
}
