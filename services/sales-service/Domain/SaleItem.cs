namespace Pos.Sales.Api.Domain;

/// <summary>
/// A single line on a sale. Product details (sku/name/price/tax) are copied from the
/// Catalog at sale time rather than referenced, keeping the Sales service autonomous.
/// </summary>
public class SaleItem
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public Sale? Sale { get; set; }

    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int Quantity { get; set; }

    /// <summary>Line price excluding tax (UnitPrice * Quantity).</summary>
    public decimal LineSubtotal { get; set; }

    /// <summary>Tax charged on this line.</summary>
    public decimal LineTax { get; set; }

    /// <summary>Line price including tax.</summary>
    public decimal LineTotal { get; set; }
}
