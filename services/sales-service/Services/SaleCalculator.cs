using Pos.Sales.Api.Domain;

namespace Pos.Sales.Api.Services;

/// <summary>Pricing input for one line, sourced from the Catalog at sale time.</summary>
public record SaleLineInput(Guid ProductId, string Sku, string Name, decimal UnitPrice, decimal TaxRate, int Quantity);

/// <summary>
/// Pure, dependency-free money math for a sale. Kept separate from controllers and EF
/// so the rounding and tax rules can be unit tested in isolation.
/// </summary>
public static class SaleCalculator
{
    public static (List<SaleItem> Items, decimal Subtotal, decimal TaxTotal, decimal GrandTotal) Build(IEnumerable<SaleLineInput> lines)
    {
        var items = new List<SaleItem>();
        decimal subtotal = 0m, taxTotal = 0m;

        foreach (var line in lines)
        {
            var lineSubtotal = Round(line.UnitPrice * line.Quantity);
            var lineTax = Round(lineSubtotal * line.TaxRate);
            var lineTotal = lineSubtotal + lineTax;

            items.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = line.ProductId,
                Sku = line.Sku,
                Name = line.Name,
                UnitPrice = line.UnitPrice,
                TaxRate = line.TaxRate,
                Quantity = line.Quantity,
                LineSubtotal = lineSubtotal,
                LineTax = lineTax,
                LineTotal = lineTotal
            });

            subtotal += lineSubtotal;
            taxTotal += lineTax;
        }

        return (items, subtotal, taxTotal, subtotal + taxTotal);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
