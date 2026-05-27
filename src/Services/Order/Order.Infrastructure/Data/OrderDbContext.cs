using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderDetailEntity> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        const string priceDecimalType = "decimal(18,2)";

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("AppOrders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Discount).HasColumnType(priceDecimalType);
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(40);
            entity.Property(e => e.UpdatedBy).HasMaxLength(40);
            entity.HasMany(e => e.OrderDetails)
                  .WithOne(d => d.Order)
                  .HasForeignKey(d => d.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderDetailEntity>(entity =>
        {
            entity.ToTable("AppOrderDetails");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType(priceDecimalType);
            entity.Property(e => e.Discount).HasColumnType(priceDecimalType);
            entity.Property(e => e.CreatedBy).HasMaxLength(40);
            entity.Property(e => e.UpdatedBy).HasMaxLength(40);
        });
    }
}
