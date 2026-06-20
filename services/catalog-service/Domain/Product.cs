namespace Pos.Catalog.Api.Domain;

/// <summary>
/// A sellable item in the cash &amp; carry catalogue. Owns its own stock level so the
/// Catalog service is the single source of truth for inventory.
/// </summary>
public class Product
{
    public Guid Id { get; set; }

    /// <summary>Human-friendly stock keeping unit, unique per product.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Scannable barcode (EAN/UPC). Optional but indexed for fast POS lookup.</summary>
    public string? Barcode { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>Price per unit excluding tax, in the store's base currency.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Tax rate applied at the till, expressed as a fraction (0.15 = 15%).</summary>
    public decimal TaxRate { get; set; }

    /// <summary>Quantity currently on hand. Decremented when the Sales service confirms a sale.</summary>
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
