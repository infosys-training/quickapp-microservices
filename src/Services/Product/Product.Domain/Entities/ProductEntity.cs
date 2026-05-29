namespace Product.Domain.Entities;

public class ProductEntity : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal BuyingPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int UnitsInStock { get; set; }
    public bool IsActive { get; set; }
    public bool IsDiscontinued { get; set; }

    public int? ParentId { get; set; }
    public ProductEntity? Parent { get; set; }

    public int ProductCategoryId { get; set; }
    public required ProductCategory ProductCategory { get; set; }

    public ICollection<ProductEntity> Children { get; } = [];
}
