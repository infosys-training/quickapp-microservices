using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Shared.Contracts.DTOs;

namespace Inventory.API.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;

    public InventoryService(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<InventoryItemDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto).ToList();
    }

    public async Task<InventoryItemDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? null : MapToDto(item);
    }

    public async Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto dto)
    {
        var item = new InventoryItem
        {
            ProductId = dto.ProductId,
            UnitsInStock = dto.UnitsInStock,
            ReorderLevel = dto.ReorderLevel,
            IsActive = dto.IsActive,
            IsDiscontinued = dto.IsDiscontinued,
            BuyingPrice = dto.BuyingPrice
        };

        var created = await _repository.AddAsync(item);
        return MapToDto(created);
    }

    public async Task<InventoryItemDto?> UpdateAsync(int id, UpdateInventoryItemDto dto)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item is null)
            return null;

        item.ProductId = dto.ProductId;
        item.UnitsInStock = dto.UnitsInStock;
        item.ReorderLevel = dto.ReorderLevel;
        item.IsActive = dto.IsActive;
        item.IsDiscontinued = dto.IsDiscontinued;
        item.BuyingPrice = dto.BuyingPrice;

        await _repository.UpdateAsync(item);
        return MapToDto(item);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item is null)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    private static InventoryItemDto MapToDto(InventoryItem item)
    {
        return new InventoryItemDto(
            item.Id,
            item.ProductId,
            item.UnitsInStock,
            item.ReorderLevel,
            item.IsActive,
            item.IsDiscontinued,
            item.BuyingPrice,
            item.CreatedDate,
            item.UpdatedDate
        );
    }
}
