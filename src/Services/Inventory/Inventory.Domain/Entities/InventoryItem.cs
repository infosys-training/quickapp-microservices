namespace Inventory.Domain.Entities;

public class InventoryItem : BaseEntity
{
    public int ProductId { get; set; }
    public int UnitsInStock { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public bool IsDiscontinued { get; set; }
    public decimal BuyingPrice { get; set; }
}
