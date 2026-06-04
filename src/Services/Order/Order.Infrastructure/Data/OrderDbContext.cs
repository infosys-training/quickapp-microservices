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
        const string priceDecimalType = "TEXT";

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("AppOrders");
            entity.Property(o => o.Comments).HasMaxLength(500);
            entity.Property(o => o.Discount).HasColumnType(priceDecimalType);
            entity.HasMany(o => o.OrderDetails)
                  .WithOne(d => d.Order)
                  .HasForeignKey(d => d.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderDetailEntity>(entity =>
        {
            entity.ToTable("AppOrderDetails");
            entity.Property(d => d.UnitPrice).HasColumnType(priceDecimalType);
            entity.Property(d => d.Discount).HasColumnType(priceDecimalType);
        });
    }

    public override int SaveChanges()
    {
        AddAuditInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddAuditInfo()
    {
        var modifiedEntries = ChangeTracker.Entries()
            .Where(x => x.Entity is IAuditableEntity &&
                       (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entry in modifiedEntries)
        {
            var entity = (IAuditableEntity)entry.Entity;
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedDate = now;
            }

            entity.UpdatedDate = now;
        }
    }
}
