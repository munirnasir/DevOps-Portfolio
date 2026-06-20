using Pos.Sales.Api.Services;
using Xunit;

namespace Pos.Sales.Tests;

public class SaleCalculatorTests
{
    [Fact]
    public void Build_SingleTaxedLine_ComputesSubtotalTaxAndTotal()
    {
        var lines = new[]
        {
            new SaleLineInput(Guid.NewGuid(), "BEV-001", "Cola 1.5L", 1.20m, 0.15m, 3)
        };

        var (items, subtotal, taxTotal, grandTotal) = SaleCalculator.Build(lines);

        var item = Assert.Single(items);
        Assert.Equal(3.60m, item.LineSubtotal);
        Assert.Equal(0.54m, item.LineTax);
        Assert.Equal(4.14m, item.LineTotal);
        Assert.Equal(3.60m, subtotal);
        Assert.Equal(0.54m, taxTotal);
        Assert.Equal(4.14m, grandTotal);
    }

    [Fact]
    public void Build_ZeroTaxLine_AddsNoTax()
    {
        var lines = new[]
        {
            new SaleLineInput(Guid.NewGuid(), "DAI-001", "Whole Milk 2L", 1.45m, 0.00m, 2)
        };

        var (_, subtotal, taxTotal, grandTotal) = SaleCalculator.Build(lines);

        Assert.Equal(2.90m, subtotal);
        Assert.Equal(0.00m, taxTotal);
        Assert.Equal(2.90m, grandTotal);
    }

    [Fact]
    public void Build_MultipleLines_AggregatesTotals()
    {
        var lines = new[]
        {
            new SaleLineInput(Guid.NewGuid(), "BEV-001", "Cola 1.5L", 1.20m, 0.15m, 2),
            new SaleLineInput(Guid.NewGuid(), "DAI-001", "Whole Milk 2L", 1.45m, 0.00m, 1)
        };

        var (items, subtotal, taxTotal, grandTotal) = SaleCalculator.Build(lines);

        Assert.Equal(2, items.Count);
        Assert.Equal(3.85m, subtotal);   // 2.40 + 1.45
        Assert.Equal(0.36m, taxTotal);   // 0.36 + 0.00
        Assert.Equal(4.21m, grandTotal);
    }

    [Fact]
    public void Build_RoundsTaxToTwoDecimals_AwayFromZero()
    {
        // 0.99 * 0.15 = 0.1485 -> rounds to 0.15
        var lines = new[]
        {
            new SaleLineInput(Guid.NewGuid(), "SNK-001", "Crisps", 0.99m, 0.15m, 1)
        };

        var (_, _, taxTotal, grandTotal) = SaleCalculator.Build(lines);

        Assert.Equal(0.15m, taxTotal);
        Assert.Equal(1.14m, grandTotal);
    }
}
