using Shared.Contracts.DTOs;

namespace Product.API.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);

    Task<IReadOnlyList<ProductCategoryDto>> GetAllCategoriesAsync();
    Task<ProductCategoryDto?> GetCategoryByIdAsync(int id);
    Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryDto dto);
    Task<ProductCategoryDto?> UpdateCategoryAsync(int id, UpdateProductCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
}
