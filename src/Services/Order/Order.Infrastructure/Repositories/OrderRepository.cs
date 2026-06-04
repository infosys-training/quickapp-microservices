using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using Order.Domain.Interfaces;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderEntity>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
    }

    public async Task<OrderEntity?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<OrderEntity> CreateAsync(OrderEntity order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<OrderEntity?> UpdateAsync(int id, OrderEntity order)
    {
        var existing = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (existing == null)
            return null;

        existing.Discount = order.Discount;
        existing.Comments = order.Comments;
        existing.CashierId = order.CashierId;
        existing.CustomerId = order.CustomerId;

        existing.OrderDetails.Clear();
        foreach (var detail in order.OrderDetails)
        {
            existing.OrderDetails.Add(new OrderDetailEntity
            {
                UnitPrice = detail.UnitPrice,
                Quantity = detail.Quantity,
                Discount = detail.Discount,
                ProductId = detail.ProductId
            });
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }
}
