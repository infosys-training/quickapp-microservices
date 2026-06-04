namespace Order.Domain.Entities;

public class OrderEntity : BaseEntity
{
    public decimal Discount { get; set; }
    public string? Comments { get; set; }
    public string? CashierId { get; set; }
    public int CustomerId { get; set; }

    public ICollection<OrderDetailEntity> OrderDetails { get; set; } = [];
}
