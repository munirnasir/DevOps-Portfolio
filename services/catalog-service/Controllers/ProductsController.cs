using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Catalog.Api.Data;
using Pos.Catalog.Api.Domain;
using Pos.Catalog.Api.Dtos;
using Pos.Catalog.Api.Security;

namespace Pos.Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly CatalogDbContext _db;

    public ProductsController(CatalogDbContext db) => _db = db;

    /// <summary>
    /// List products, optionally filtered by category or free-text search. Paged:
    /// pass <c>page</c> (1-based) and <c>pageSize</c> (1–100, default 20).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Products.Include(p => p.Category).AsNoTracking();

        if (categoryId is not null)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{term}%") ||
                EF.Functions.ILike(p.Sku, $"%{term}%"));
        }

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToDto(p))
            .ToListAsync();

        return Ok(new PagedResult<ProductDto>(products, page, pageSize, totalCount));
    }

    /// <summary>Fetch a single product by its identifier.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _db.Products.Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? NotFound() : Ok(ToDto(product));
    }

    /// <summary>Lookup by barcode — the primary path used when scanning at the till.</summary>
    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ProductDto>> GetByBarcode(string barcode)
    {
        var product = await _db.Products.Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);

        return product is null ? NotFound() : Ok(ToDto(product));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Manager)]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductRequest request)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
        {
            return Problem($"Category {request.CategoryId} does not exist.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (await _db.Products.AnyAsync(p => p.Sku == request.Sku))
        {
            return Conflict($"A product with SKU '{request.Sku}' already exists.");
        }

        var now = DateTime.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Sku = request.Sku,
            Barcode = request.Barcode,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitPrice = request.UnitPrice,
            TaxRate = request.TaxRate,
            StockQuantity = request.StockQuantity,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ToDto(product));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductRequest request)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
        {
            return Problem($"Category {request.CategoryId} does not exist.", statusCode: StatusCodes.Status400BadRequest);
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.UnitPrice = request.UnitPrice;
        product.TaxRate = request.TaxRate;
        product.IsActive = request.IsActive;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();

        return Ok(ToDto(product));
    }

    /// <summary>
    /// Adjust stock on hand. Called both by back-office restocking and by the Sales
    /// service when a sale is confirmed (with a negative quantity).
    /// </summary>
    [HttpPost("{id:guid}/stock-adjustment")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<ActionResult<ProductDto>> AdjustStock(Guid id, StockAdjustmentRequest request)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        var newQuantity = product.StockQuantity + request.QuantityChange;
        if (newQuantity < 0)
        {
            return Problem(
                $"Insufficient stock for '{product.Name}'. On hand: {product.StockQuantity}, requested change: {request.QuantityChange}.",
                statusCode: StatusCodes.Status409Conflict);
        }

        product.StockQuantity = newQuantity;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ToDto(product));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        // Soft delete keeps historical sales references intact.
        product.IsActive = false;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static ProductDto ToDto(Product p) => new(
        p.Id, p.Sku, p.Barcode, p.Name, p.Description,
        p.CategoryId, p.Category?.Name ?? string.Empty,
        p.UnitPrice, p.TaxRate, p.StockQuantity, p.IsActive);
}
