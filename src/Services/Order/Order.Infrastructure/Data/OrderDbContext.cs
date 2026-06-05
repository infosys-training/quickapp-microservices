using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        const string priceDecimalType = "decimal(18,2)";

        modelBuilder.Entity<Domain.Entities.Order>(entity =>
        {
            entity.ToTable("AppOrders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.Discount).HasColumnType(priceDecimalType);
            entity.Property(e => e.CreatedBy).HasMaxLength(40);
            entity.Property(e => e.UpdatedBy).HasMaxLength(40);

            entity.HasMany(e => e.OrderDetails)
                .WithOne(d => d.Order)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.ToTable("AppOrderDetails");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType(priceDecimalType);
            entity.Property(e => e.Discount).HasColumnType(priceDecimalType);
            entity.Property(e => e.CreatedBy).HasMaxLength(40);
            entity.Property(e => e.UpdatedBy).HasMaxLength(40);
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
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Domain.Entities.Order>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
                entry.Entity.UpdatedDate = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedDate = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<OrderDetail>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
                entry.Entity.UpdatedDate = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedDate = now;
            }
        }
    }
}
