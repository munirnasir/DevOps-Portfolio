using System.ComponentModel.DataAnnotations;

namespace Pos.Catalog.Api.Dtos;

public record ProductDto(
    Guid Id,
    string Sku,
    string? Barcode,
    string Name,
    string? Description,
    int CategoryId,
    string CategoryName,
    decimal UnitPrice,
    decimal TaxRate,
    int StockQuantity,
    bool IsActive);

public record CreateProductRequest(
    [Required, MaxLength(50)] string Sku,
    [MaxLength(50)] string? Barcode,
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(1, int.MaxValue)] int CategoryId,
    [Range(0, 1_000_000)] decimal UnitPrice,
    [Range(0, 1)] decimal TaxRate,
    [Range(0, int.MaxValue)] int StockQuantity);

public record UpdateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(1, int.MaxValue)] int CategoryId,
    [Range(0, 1_000_000)] decimal UnitPrice,
    [Range(0, 1)] decimal TaxRate,
    bool IsActive);

/// <summary>Positive quantities add stock (restock); negative quantities remove it.</summary>
public record StockAdjustmentRequest(
    [Required] int QuantityChange,
    [MaxLength(200)] string? Reason);

public record CategoryDto(int Id, string Name, string? Description);

public record CreateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description);
