using Order.Domain.Entities;
using Order.Domain.Interfaces;
using Shared.Contracts.DTOs;

namespace Order.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order == null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        var order = new OrderEntity
        {
            Discount = dto.Discount,
            Comments = dto.Comments,
            CashierId = dto.CashierId,
            CustomerId = dto.CustomerId,
            OrderDetails = dto.OrderDetails.Select(d => new OrderDetailEntity
            {
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                Discount = d.Discount,
                ProductId = d.ProductId
            }).ToList()
        };

        var created = await _orderRepository.CreateAsync(order);
        return MapToDto(created);
    }

    public async Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto dto)
    {
        var order = new OrderEntity
        {
            Id = id,
            Discount = dto.Discount,
            Comments = dto.Comments,
            CashierId = dto.CashierId,
            CustomerId = dto.CustomerId,
            OrderDetails = dto.OrderDetails.Select(d => new OrderDetailEntity
            {
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                Discount = d.Discount,
                ProductId = d.ProductId
            }).ToList()
        };

        var updated = await _orderRepository.UpdateAsync(order);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        return await _orderRepository.DeleteAsync(id);
    }

    private static OrderDto MapToDto(OrderEntity entity)
    {
        return new OrderDto(
            entity.Id,
            entity.Discount,
            entity.Comments,
            entity.CashierId,
            entity.CustomerId,
            entity.CreatedDate,
            entity.UpdatedDate,
            entity.OrderDetails.Select(d => new OrderDetailDto(
                d.Id,
                d.UnitPrice,
                d.Quantity,
                d.Discount,
                d.ProductId,
                d.OrderId
            )).ToList()
        );
    }
}
