using Order.Domain.Entities;
using Order.Domain.Interfaces;
using Shared.Contracts.DTOs;

namespace Order.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _repository.GetAllAsync();
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        return order is null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId)
    {
        var orders = await _repository.GetByCustomerIdAsync(customerId);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        var order = new Domain.Entities.Order
        {
            Discount = dto.Discount,
            Comments = dto.Comments,
            CashierId = dto.CashierId,
            CustomerId = dto.CustomerId,
            OrderDetails = dto.OrderDetails.Select(d => new OrderDetail
            {
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                Discount = d.Discount,
                ProductId = d.ProductId
            }).ToList()
        };

        var created = await _repository.CreateAsync(order);
        return MapToDto(created);
    }

    public async Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto dto)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order is null) return null;

        order.Discount = dto.Discount;
        order.Comments = dto.Comments;
        order.CashierId = dto.CashierId;
        order.CustomerId = dto.CustomerId;

        await _repository.UpdateAsync(order);
        return MapToDto(order);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order is null) return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    private static OrderDto MapToDto(Domain.Entities.Order order) => new(
        order.Id,
        order.Discount,
        order.Comments,
        order.CashierId,
        order.CustomerId,
        order.CreatedDate,
        order.UpdatedDate,
        order.OrderDetails.Select(d => new OrderDetailDto(
            d.Id,
            d.UnitPrice,
            d.Quantity,
            d.Discount,
            d.ProductId,
            d.OrderId
        )).ToList()
    );
}
