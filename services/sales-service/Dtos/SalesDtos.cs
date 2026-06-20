using System.ComponentModel.DataAnnotations;
using Pos.Sales.Api.Domain;

namespace Pos.Sales.Api.Dtos;

/// <summary>A page of results plus the totals a client needs to render pager controls.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public record CreateSaleItemRequest(
    [Required] Guid ProductId,
    [Range(1, 1000)] int Quantity);

public record CreateSaleRequest(
    [Required, MinLength(1)] List<CreateSaleItemRequest> Items,
    [MaxLength(100)] string? CashierName,
    [Required] PaymentMethod PaymentMethod,
    [Range(0, 1_000_000)] decimal AmountTendered);

public record SaleItemDto(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    decimal TaxRate,
    int Quantity,
    decimal LineSubtotal,
    decimal LineTax,
    decimal LineTotal);

public record SaleDto(
    Guid Id,
    long Number,
    string? CashierName,
    DateTime CreatedAtUtc,
    decimal Subtotal,
    decimal TaxTotal,
    decimal GrandTotal,
    string PaymentMethod,
    decimal AmountTendered,
    decimal ChangeDue,
    IReadOnlyList<SaleItemDto> Items);
