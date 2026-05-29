using Product.Domain.Entities;
using Product.Domain.Interfaces;
using Shared.Contracts.DTOs;

namespace Product.API.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllProductsAsync()
    {
        var products = await _repository.GetAllProductsAsync();
        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetProductByIdAsync(id);
        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var category = await _repository.GetCategoryByIdAsync(dto.ProductCategoryId)
            ?? throw new ArgumentException($"Category with ID {dto.ProductCategoryId} not found.");

        var product = new ProductEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            BuyingPrice = dto.BuyingPrice,
            SellingPrice = dto.SellingPrice,
            UnitsInStock = dto.UnitsInStock,
            IsActive = dto.IsActive,
            IsDiscontinued = dto.IsDiscontinued,
            ProductCategoryId = dto.ProductCategoryId,
            ProductCategory = category,
            ParentId = dto.ParentId
        };

        var created = await _repository.AddProductAsync(product);
        return MapToDto(created);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _repository.GetProductByIdAsync(id);
        if (product is null) return null;

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Icon = dto.Icon;
        product.BuyingPrice = dto.BuyingPrice;
        product.SellingPrice = dto.SellingPrice;
        product.UnitsInStock = dto.UnitsInStock;
        product.IsActive = dto.IsActive;
        product.IsDiscontinued = dto.IsDiscontinued;
        product.ProductCategoryId = dto.ProductCategoryId;
        product.ParentId = dto.ParentId;

        await _repository.UpdateProductAsync(product);

        var updated = await _repository.GetProductByIdAsync(id);
        return updated is null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _repository.GetProductByIdAsync(id);
        if (product is null) return false;

        await _repository.DeleteProductAsync(product);
        return true;
    }

    public async Task<IReadOnlyList<ProductCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _repository.GetAllCategoriesAsync();
        return categories.Select(MapCategoryToDto).ToList();
    }

    public async Task<ProductCategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        return category is null ? null : MapCategoryToDto(category);
    }

    public async Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryDto dto)
    {
        var category = new ProductCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon
        };

        var created = await _repository.AddCategoryAsync(category);
        return MapCategoryToDto(created);
    }

    public async Task<ProductCategoryDto?> UpdateCategoryAsync(int id, UpdateProductCategoryDto dto)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        if (category is null) return null;

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.Icon = dto.Icon;

        await _repository.UpdateCategoryAsync(category);
        return MapCategoryToDto(category);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        if (category is null) return false;

        await _repository.DeleteCategoryAsync(category);
        return true;
    }

    private static ProductDto MapToDto(ProductEntity product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Icon,
        product.BuyingPrice,
        product.SellingPrice,
        product.UnitsInStock,
        product.IsActive,
        product.IsDiscontinued,
        product.ProductCategoryId,
        product.ProductCategory?.Name,
        product.ParentId);

    private static ProductCategoryDto MapCategoryToDto(ProductCategory category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.Icon);
}
