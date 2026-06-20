using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Sales.Api.Data;
using Pos.Sales.Api.Domain;
using Pos.Sales.Api.Dtos;
using Pos.Sales.Api.Services;

namespace Pos.Sales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly SalesDbContext _db;
    private readonly ICatalogClient _catalog;
    private readonly ILogger<SalesController> _logger;

    public SalesController(SalesDbContext db, ICatalogClient catalog, ILogger<SalesController> logger)
    {
        _db = db;
        _catalog = catalog;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetSales([FromQuery] int take = 50)
    {
        var sales = await _db.Sales
            .Include(s => s.Items)
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAtUtc)
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync();

        return Ok(sales.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleDto>> GetSale(Guid id)
    {
        var sale = await _db.Sales.Include(s => s.Items).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return sale is null ? NotFound() : Ok(ToDto(sale));
    }

    /// <summary>Plain-text receipt suitable for a till printer.</summary>
    [HttpGet("{id:guid}/receipt")]
    [Produces("text/plain")]
    public async Task<IActionResult> GetReceipt(Guid id)
    {
        var sale = await _db.Sales.Include(s => s.Items).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return sale is null ? NotFound() : Content(RenderReceipt(sale), "text/plain");
    }

    /// <summary>
    /// Rings up a sale: validates each product and its stock against the Catalog,
    /// computes totals, decrements Catalog stock, then persists the transaction.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SaleDto>> CreateSale(CreateSaleRequest request)
    {
        // Merge duplicate product lines into a single quantity.
        var requestedQuantities = request.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var lines = new List<SaleLineInput>();
        foreach (var (productId, quantity) in requestedQuantities)
        {
            var product = await _catalog.GetProductAsync(productId);
            if (product is null || !product.IsActive)
            {
                return Problem($"Product {productId} is not available.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (product.StockQuantity < quantity)
            {
                return Problem(
                    $"Insufficient stock for '{product.Name}'. On hand: {product.StockQuantity}, requested: {quantity}.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            lines.Add(new SaleLineInput(product.Id, product.Sku, product.Name, product.UnitPrice, product.TaxRate, quantity));
        }

        var (items, subtotal, taxTotal, grandTotal) = SaleCalculator.Build(lines);

        var tendered = request.PaymentMethod == PaymentMethod.Cash ? request.AmountTendered : grandTotal;
        if (tendered < grandTotal)
        {
            return Problem(
                $"Amount tendered ({tendered:0.00}) is less than the total due ({grandTotal:0.00}).",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Decrement Catalog stock. No distributed transaction exists, so compensate
        // (restock) anything already deducted if a later line fails.
        var deducted = new List<(Guid ProductId, int Quantity)>();
        foreach (var line in lines)
        {
            var ok = await _catalog.AdjustStockAsync(line.ProductId, -line.Quantity, "POS sale");
            if (!ok)
            {
                foreach (var (pid, qty) in deducted)
                {
                    await _catalog.AdjustStockAsync(pid, qty, "Compensating failed sale");
                }
                return Problem(
                    $"Stock for '{line.Name}' changed during checkout. Please retry.",
                    statusCode: StatusCodes.Status409Conflict);
            }
            deducted.Add((line.ProductId, line.Quantity));
        }

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            CashierName = request.CashierName,
            CreatedAtUtc = DateTime.UtcNow,
            Subtotal = subtotal,
            TaxTotal = taxTotal,
            GrandTotal = grandTotal,
            PaymentMethod = request.PaymentMethod,
            AmountTendered = tendered,
            ChangeDue = tendered - grandTotal,
            Items = items
        };

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, ToDto(sale));
    }

    private static SaleDto ToDto(Sale s) => new(
        s.Id, s.Number, s.CashierName, s.CreatedAtUtc,
        s.Subtotal, s.TaxTotal, s.GrandTotal,
        s.PaymentMethod.ToString(), s.AmountTendered, s.ChangeDue,
        s.Items.Select(i => new SaleItemDto(
            i.ProductId, i.Sku, i.Name, i.UnitPrice, i.TaxRate, i.Quantity,
            i.LineSubtotal, i.LineTax, i.LineTotal)).ToList());

    private static string RenderReceipt(Sale s)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        CASH & CARRY POS");
        sb.AppendLine("===============================");
        sb.AppendLine($"Receipt #: {s.Number}");
        sb.AppendLine($"Date     : {s.CreatedAtUtc:yyyy-MM-dd HH:mm} UTC");
        if (!string.IsNullOrWhiteSpace(s.CashierName))
        {
            sb.AppendLine($"Cashier  : {s.CashierName}");
        }
        sb.AppendLine("-------------------------------");
        foreach (var i in s.Items)
        {
            sb.AppendLine(i.Name);
            var left = $"  {i.Quantity} x {i.UnitPrice:0.00}";
            var right = i.LineTotal.ToString("0.00");
            sb.AppendLine(left + right.PadLeft(Math.Max(1, 31 - left.Length)));
        }
        sb.AppendLine("-------------------------------");
        sb.AppendLine($"Subtotal {s.Subtotal,21:0.00}");
        sb.AppendLine($"Tax      {s.TaxTotal,21:0.00}");
        sb.AppendLine($"TOTAL    {s.GrandTotal,21:0.00}");
        sb.AppendLine($"{s.PaymentMethod,-8} {s.AmountTendered,21:0.00}");
        sb.AppendLine($"Change   {s.ChangeDue,21:0.00}");
        sb.AppendLine("===============================");
        sb.AppendLine("     Thank you for shopping!");
        return sb.ToString();
    }
}
