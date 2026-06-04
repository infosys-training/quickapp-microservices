using Order.Domain.Entities;

namespace Order.Domain.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<OrderEntity>> GetAllAsync();
    Task<OrderEntity?> GetByIdAsync(int id);
    Task<OrderEntity> CreateAsync(OrderEntity order);
    Task<OrderEntity?> UpdateAsync(int id, OrderEntity order);
    Task<bool> DeleteAsync(int id);
}
