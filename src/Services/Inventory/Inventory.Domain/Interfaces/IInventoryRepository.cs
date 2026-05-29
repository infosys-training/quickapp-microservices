using Inventory.Domain.Entities;

namespace Inventory.Domain.Interfaces;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<IReadOnlyList<InventoryItem>> GetAllAsync();
    Task<InventoryItem> AddAsync(InventoryItem item);
    Task UpdateAsync(InventoryItem item);
    Task DeleteAsync(int id);
}
