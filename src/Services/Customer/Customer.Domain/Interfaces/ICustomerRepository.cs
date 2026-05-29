using Customer.Domain.Entities;

namespace Customer.Domain.Interfaces;

public interface ICustomerRepository
{
    Task<IReadOnlyList<CustomerEntity>> GetAllAsync();
    Task<CustomerEntity?> GetByIdAsync(int id);
    Task<CustomerEntity> AddAsync(CustomerEntity customer);
    Task UpdateAsync(CustomerEntity customer);
    Task DeleteAsync(int id);
}
