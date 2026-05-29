using Product.Domain.Entities;

namespace Product.Domain.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<ProductEntity>> GetAllProductsAsync();
    Task<ProductEntity?> GetProductByIdAsync(int id);
    Task<ProductEntity> AddProductAsync(ProductEntity product);
    Task UpdateProductAsync(ProductEntity product);
    Task DeleteProductAsync(ProductEntity product);

    Task<IReadOnlyList<ProductCategory>> GetAllCategoriesAsync();
    Task<ProductCategory?> GetCategoryByIdAsync(int id);
    Task<ProductCategory> AddCategoryAsync(ProductCategory category);
    Task UpdateCategoryAsync(ProductCategory category);
    Task DeleteCategoryAsync(ProductCategory category);
}
