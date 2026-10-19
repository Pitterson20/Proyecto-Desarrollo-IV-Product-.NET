using Ferreteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferreteria.Data;

/// <summary>
/// Datos de prueba equivalentes a los del proyecto NestJS: cinco productos,
/// cuatro categorías, cinco marcas, cuatro unidades y tres usuarios.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDBContext db, CancellationToken cancellationToken = default)
    {
        await SeedUsersAsync(db, cancellationToken);
        await SeedProductsAsync(db, cancellationToken);
    }

    private static async Task SeedUsersAsync(AppDBContext db, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password", workFactor: 10);

        db.Users.AddRange(
            new User
            {
                Name = "Administrador",
                Email = "admin@example.com",
                Password = passwordHash,
                Role = UserRole.Admin
            },
            new User
            {
                Name = "Cajero",
                Email = "cajero@example.com",
                Password = passwordHash,
                Role = UserRole.Cajero
            },
            new User
            {
                Name = "Bodeguero",
                Email = "bodeguero@example.com",
                Password = passwordHash,
                Role = UserRole.Bodeguero
            });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedProductsAsync(AppDBContext db, CancellationToken cancellationToken)
    {
        var hasProducts = await db.Products
            .IgnoreQueryFilters()
            .AnyAsync(cancellationToken);

        if (hasProducts)
        {
            return;
        }

        var categories = new[]
        {
            new Category { CategoryName = "Herramientas manuales", Description = "Herramientas de uso manual" },
            new Category { CategoryName = "Herramientas eléctricas", Description = "Herramientas que requieren energía eléctrica" },
            new Category { CategoryName = "Pintura", Description = "Pinturas y recubrimientos" },
            new Category { CategoryName = "Construcción", Description = "Materiales de construcción" }
        };

        var brands = new[]
        {
            new Brand { BrandName = "Truper" },
            new Brand { BrandName = "DeWalt" },
            new Brand { BrandName = "Sherwin-Williams" },
            new Brand { BrandName = "Holcim" },
            new Brand { BrandName = "Stanley" }
        };

        var units = new[]
        {
            new Unit { UnitName = "Unidad" },
            new Unit { UnitName = "Galón" },
            new Unit { UnitName = "Saco" },
            new Unit { UnitName = "Juego" }
        };

        db.Categories.AddRange(categories);
        db.Brands.AddRange(brands);
        db.Units.AddRange(units);
        await db.SaveChangesAsync(cancellationToken);

        db.Products.AddRange(
            new Product
            {
                CategoryId = categories[0].Id,
                BrandId = brands[0].Id,
                UnitId = units[0].Id,
                Name = "Martillo 16oz",
                Description = "Martillo de acero con mango de fibra",
                Sku = "SKU-001",
                Barcode = "7501000000019",
                Price = 12500.00m,
                TaxRate = 0.13m,
                StockQuantity = 45,
                MinimumStock = 10,
                MaximumStock = 100,
                Weight = 0.45m,
                IsActive = true
            },
            new Product
            {
                CategoryId = categories[1].Id,
                BrandId = brands[1].Id,
                UnitId = units[0].Id,
                Name = "Taladro 750W",
                Description = "Taladro percutor 750W",
                Sku = "SKU-002",
                Barcode = "7501000000026",
                Price = 85000.00m,
                TaxRate = 0.13m,
                StockQuantity = 20,
                MinimumStock = 5,
                MaximumStock = 50,
                Weight = 2.10m,
                IsActive = true
            },
            new Product
            {
                CategoryId = categories[2].Id,
                BrandId = brands[2].Id,
                UnitId = units[1].Id,
                Name = "Pintura blanca 1gal",
                Description = "Pintura acrílica blanca",
                Sku = "SKU-003",
                Barcode = "7501000000033",
                Price = 35000.00m,
                TaxRate = 0.13m,
                StockQuantity = 60,
                MinimumStock = 15,
                MaximumStock = 120,
                Weight = 4.00m,
                IsActive = true
            },
            new Product
            {
                CategoryId = categories[3].Id,
                BrandId = brands[3].Id,
                UnitId = units[2].Id,
                Name = "Cemento Holcim 50kg",
                Description = "Cemento gris de uso general",
                Sku = "SKU-004",
                Barcode = "7501000000040",
                Price = 9500.00m,
                TaxRate = 0.13m,
                StockQuantity = 200,
                MinimumStock = 50,
                MaximumStock = 500,
                Weight = 50.00m,
                IsActive = true
            },
            new Product
            {
                CategoryId = categories[0].Id,
                BrandId = brands[4].Id,
                UnitId = units[3].Id,
                Name = "Juego destornilladores 6pz",
                Description = "Set de 6 destornilladores",
                Sku = "SKU-005",
                Barcode = "7501000000057",
                Price = 18000.00m,
                TaxRate = 0.13m,
                StockQuantity = 35,
                MinimumStock = 10,
                MaximumStock = 80,
                Weight = 0.80m,
                IsActive = true
            });

        await db.SaveChangesAsync(cancellationToken);
    }
}
