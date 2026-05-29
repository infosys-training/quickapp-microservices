namespace Order.Domain.Entities;

public class OrderDetailEntity : BaseEntity
{
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public int ProductId { get; set; }
    public int OrderId { get; set; }

    public OrderEntity Order { get; set; } = null!;
}
