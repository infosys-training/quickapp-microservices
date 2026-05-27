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

    public async Task<IEnumerable<OrderEntity>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
    }

    public async Task<OrderEntity> CreateAsync(OrderEntity order)
    {
        var now = DateTime.UtcNow;
        order.CreatedDate = now;
        order.UpdatedDate = now;

        foreach (var detail in order.OrderDetails)
        {
            detail.CreatedDate = now;
            detail.UpdatedDate = now;
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<OrderEntity?> UpdateAsync(OrderEntity order)
    {
        var existing = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == order.Id);
        if (existing == null)
            return null;

        existing.Discount = order.Discount;
        existing.Comments = order.Comments;
        existing.UpdatedDate = DateTime.UtcNow;

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
