using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Pos.Sales.Api.Dtos;
using Pos.Sales.Api.Security;
using Xunit;

namespace Pos.Sales.IntegrationTests;

public class SalesApiTests : IClassFixture<SalesApiFactory>
{
    private readonly SalesApiFactory _factory;

    public SalesApiTests(SalesApiFactory factory) => _factory = factory;

    private HttpClient ClientFor(string? role)
    {
        var client = _factory.CreateClient();
        if (role is not null)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestAuth.TokenFor(role));
        }
        return client;
    }

    [Fact]
    public async Task CreateSale_WithoutToken_Returns401()
    {
        var body = new { items = new[] { new { productId = Guid.NewGuid(), quantity = 1 } }, paymentMethod = "Cash", amountTendered = 10m };
        var response = await ClientFor(null).PostAsJsonAsync("/api/sales", body);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSale_AsCashier_WithStringPaymentMethod_Succeeds()
    {
        // Regression: paymentMethod is sent as the JSON string "Cash" (as the Angular
        // frontend sends it). This must bind without a 400.
        _factory.Catalog.UnitPrice = 2.00m;
        _factory.Catalog.TaxRate = 0.10m;
        _factory.Catalog.StockOnHand = 100;

        var productId = Guid.NewGuid();
        var body = new
        {
            items = new[] { new { productId, quantity = 2 } },
            cashierName = "Integration",
            paymentMethod = "Cash",
            amountTendered = 5.00m
        };

        var response = await ClientFor(Roles.Cashier).PostAsJsonAsync("/api/sales", body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var sale = await response.Content.ReadFromJsonAsync<SaleDto>();
        Assert.NotNull(sale);
        Assert.Equal(4.00m, sale!.Subtotal);   // 2.00 * 2
        Assert.Equal(0.40m, sale.TaxTotal);    // 10%
        Assert.Equal(4.40m, sale.GrandTotal);
        Assert.Equal(0.60m, sale.ChangeDue);   // 5.00 - 4.40
        Assert.Equal("Cash", sale.PaymentMethod);

        // The sale must have asked the Catalog to decrement stock by the sold quantity.
        Assert.Contains(_factory.Catalog.Adjustments, a => a.ProductId == productId && a.Change == -2);
    }
}
