using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Pos.Catalog.Api.Dtos;
using Pos.Catalog.Api.Security;
using Xunit;

namespace Pos.Catalog.IntegrationTests;

public class ProductsApiTests : IClassFixture<CatalogApiFactory>
{
    private readonly CatalogApiFactory _factory;

    public ProductsApiTests(CatalogApiFactory factory) => _factory = factory;

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

    // Local shape for deserialization (avoids interface-typed Items).
    private record PagedProducts(List<ProductDto> Items, int Page, int PageSize, int TotalCount);

    [Fact]
    public async Task GetProducts_WithoutToken_Returns401()
    {
        var response = await ClientFor(null).GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithCashierToken_ReturnsSeededProducts()
    {
        var page = await ClientFor(Roles.Cashier).GetFromJsonAsync<PagedProducts>("/api/products?pageSize=100");

        Assert.NotNull(page);
        // >= 10 rather than == 10: other tests in this class share the database and may
        // have inserted products, so assert on the seeded data being present instead.
        Assert.True(page!.TotalCount >= 10, $"expected at least 10 seeded products, got {page.TotalCount}");
        Assert.Contains(page.Items, p => p.Sku == "BEV-001");
        Assert.Equal(page.TotalCount, page.Items.Count); // pageSize=100 fits everything on one page
    }

    [Fact]
    public async Task GetProducts_Pagination_RespectsPageSize()
    {
        var page = await ClientFor(Roles.Cashier).GetFromJsonAsync<PagedProducts>("/api/products?page=1&pageSize=3");

        Assert.NotNull(page);
        Assert.Equal(3, page!.Items.Count);
        Assert.Equal(3, page.PageSize);
        Assert.True(page.TotalCount >= 10); // shared DB may hold products inserted by other tests
    }

    [Fact]
    public async Task CreateProduct_AsCashier_Returns403()
    {
        var body = new
        {
            sku = "NEW-001", name = "New Item", categoryId = 1,
            unitPrice = 1.50m, taxRate = 0.15m, stockQuantity = 10
        };

        var response = await ClientFor(Roles.Cashier).PostAsJsonAsync("/api/products", body);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_AsManager_Returns201()
    {
        var body = new
        {
            sku = "MGR-001", name = "Manager Added", categoryId = 1,
            unitPrice = 2.50m, taxRate = 0.15m, stockQuantity = 25
        };

        var response = await ClientFor(Roles.Manager).PostAsJsonAsync("/api/products", body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);
        Assert.Equal("MGR-001", created!.Sku);
        Assert.Equal(25, created.StockQuantity);
    }
}
