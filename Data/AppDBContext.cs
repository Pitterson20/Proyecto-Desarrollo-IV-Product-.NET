using Ferreteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferreteria.Data;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(product => product.Sku).IsUnique();
            entity.HasIndex(product => product.Barcode).IsUnique();

            entity.HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(product => product.Brand)
                .WithMany(brand => brand.Products)
                .HasForeignKey(product => product.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(product => product.Unit)
                .WithMany(unit => unit.Products)
                .HasForeignKey(product => product.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(product => product.DeletedAt == null);

            entity.Property(product => product.Price).HasConversion<double>();
            entity.Property(product => product.TaxRate).HasConversion<double>();
            entity.Property(product => product.Weight).HasConversion<double?>();
        });

        modelBuilder.Entity<PurchaseItem>(entity =>
        {
            entity.HasOne(item => item.Product)
                .WithMany(product => product.PurchaseItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Role)
                .HasConversion(
                    role => role.ToString().ToLowerInvariant(),
                    value => Enum.Parse<UserRole>(value, true));
        });
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
