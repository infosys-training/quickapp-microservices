using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using Order.Domain.Interfaces;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _dbContext;

    public OrderRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<OrderEntity>> GetAllAsync()
    {
        return await _dbContext.Orders
            .Include(o => o.OrderDetails)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
    }

    public async Task<OrderEntity?> GetByIdAsync(int id)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<OrderEntity>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderDetails)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
    }

    public async Task<OrderEntity> CreateAsync(OrderEntity order)
    {
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        return order;
    }

    public async Task<OrderEntity?> UpdateAsync(OrderEntity order)
    {
        var existing = await _dbContext.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

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
                ProductId = detail.ProductId,
                OrderId = existing.Id
            });
        }

        await _dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order == null)
            return false;

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
