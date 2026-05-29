namespace Shared.Contracts.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string? Description,
    string? Icon,
    decimal BuyingPrice,
    decimal SellingPrice,
    int UnitsInStock,
    bool IsActive,
    bool IsDiscontinued,
    int ProductCategoryId,
    string? ProductCategoryName,
    int? ParentId);

public record CreateProductDto(
    string Name,
    string? Description,
    string? Icon,
    decimal BuyingPrice,
    decimal SellingPrice,
    int UnitsInStock,
    bool IsActive,
    bool IsDiscontinued,
    int ProductCategoryId,
    int? ParentId);

public record UpdateProductDto(
    string Name,
    string? Description,
    string? Icon,
    decimal BuyingPrice,
    decimal SellingPrice,
    int UnitsInStock,
    bool IsActive,
    bool IsDiscontinued,
    int ProductCategoryId,
    int? ParentId);

public record ProductCategoryDto(
    int Id,
    string Name,
    string? Description,
    string? Icon);

public record CreateProductCategoryDto(
    string Name,
    string? Description,
    string? Icon);

public record UpdateProductCategoryDto(
    string Name,
    string? Description,
    string? Icon);
