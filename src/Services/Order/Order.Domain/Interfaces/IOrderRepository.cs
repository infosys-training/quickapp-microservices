using Order.Domain.Entities;

namespace Order.Domain.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<Entities.Order>> GetAllAsync();
    Task<Entities.Order?> GetByIdAsync(int id);
    Task<IEnumerable<Entities.Order>> GetByCustomerIdAsync(int customerId);
    Task<Entities.Order> CreateAsync(Entities.Order order);
    Task UpdateAsync(Entities.Order order);
    Task DeleteAsync(int id);
}
