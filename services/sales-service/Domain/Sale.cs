namespace Pos.Sales.Api.Domain;

public enum PaymentMethod
{
    Cash = 0,
    Card = 1
}

/// <summary>
/// A completed point-of-sale transaction. Totals are snapshotted at sale time so a
/// later price change in the Catalog never alters historical receipts.
/// </summary>
public class Sale
{
    public Guid Id { get; set; }

    /// <summary>Sequential, human-friendly receipt number shown to the customer.</summary>
    public long Number { get; set; }

    public string? CashierName { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountTendered { get; set; }
    public decimal ChangeDue { get; set; }

    public List<SaleItem> Items { get; set; } = new();
}
