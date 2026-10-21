using System.Globalization;
using Ferreteria.Common.Errors;
using Ferreteria.Data;
using Ferreteria.DTOs.Common;
using Ferreteria.DTOs.Products;
using Ferreteria.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Ferreteria.Services;

/// <summary>
/// Réplica de ProductService.php y ProductsService de NestJS: filtros,
/// paginación, reglas R1/R2, borrado lógico y validaciones de catálogo.
/// </summary>
public sealed class ProductService
{
    private const string ProductNotFoundMessage = "El producto no fue encontrado.";
    private const decimal DefaultTaxRate = 0.13m;
    private const int DefaultMinimumStock = 1;

    private static readonly HashSet<string> AllowedSorts = new(StringComparer.Ordinal)
    {
        "name",
        "price",
        "stock_quantity",
        "id",
        "created_at"
    };

    private readonly AppDBContext _db;

    public ProductService(AppDBContext db)
    {
        _db = db;
    }

    public async Task<PaginatedResponse<ProductResponse>> ListAsync(
        ProductQuery query,
        PathString path,
        IQueryCollection queryString,
        CancellationToken cancellationToken = default)
    {
        var page = PositiveInt(query.Page, 1);
        var perPage = Math.Min(50, PositiveInt(query.PerPage, 10));
        var sort = AllowedSorts.Contains(query.Sort ?? string.Empty)
            ? query.Sort!
            : "id";
        var descending = string.Equals(
            query.Direction,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        IQueryable<Product> products = _db.Products.AsNoTracking();

        if (!string.IsNullOrEmpty(query.Q))
        {
            var pattern = $"%{query.Q}%";

            products = products.Where(product =>
                EF.Functions.Like(product.Name, pattern)
                || EF.Functions.Like(product.Sku, pattern)
                || (product.Barcode != null
                    && EF.Functions.Like(product.Barcode, pattern)));
        }

        if (TryParseInt(query.CategoryId, out var categoryId))
        {
            products = products.Where(product => product.CategoryId == categoryId);
        }

        if (TryParseInt(query.BrandId, out var brandId))
        {
            products = products.Where(product => product.BrandId == brandId);
        }

        if (TryParseBoolean(query.IsActive, out var isActive))
        {
            products = products.Where(product => product.IsActive == isActive);
        }

        if (TryParseBoolean(query.LowStock, out var lowStock) && lowStock)
        {
            products = products.Where(product =>
                product.StockQuantity <= product.MinimumStock);
        }

        if (TryParseDecimal(query.MinPrice, out var minPrice))
        {
            products = products.Where(product => product.Price >= minPrice);
        }

        if (TryParseDecimal(query.MaxPrice, out var maxPrice))
        {
            products = products.Where(product => product.Price <= maxPrice);
        }

        var total = await products.CountAsync(cancellationToken);

        var ordered = (sort, descending) switch
        {
            ("name", false) => products.OrderBy(product => product.Name),
            ("name", true) => products.OrderByDescending(product => product.Name),
            ("price", false) => products.OrderBy(product => product.Price),
            ("price", true) => products.OrderByDescending(product => product.Price),
            ("stock_quantity", false) =>
                products.OrderBy(product => product.StockQuantity),
            ("stock_quantity", true) =>
                products.OrderByDescending(product => product.StockQuantity),
            ("created_at", false) =>
                products.OrderBy(product => product.CreatedAt),
            ("created_at", true) =>
                products.OrderByDescending(product => product.CreatedAt),
            ("id", true) => products.OrderByDescending(product => product.Id),
            _ => products.OrderBy(product => product.Id)
        };

        if (sort != "id")
        {
            ordered = ordered.ThenBy(product => product.Id);
        }

        var items = await ordered
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        var lastPage = Math.Max(1, (int)Math.Ceiling(total / (double)perPage));
        var basePath = path.Value ?? "/api/products";
        var from = items.Count == 0 ? null : (int?)(((page - 1) * perPage) + 1);
        var to = items.Count == 0 ? null : (int?)(from!.Value + items.Count - 1);

        return new PaginatedResponse<ProductResponse>
        {
            Data = items.Select(ProductResponse.FromEntity).ToArray(),
            Links = new PaginationLinks
            {
                First = BuildPageLink(basePath, queryString, 1),
                Last = BuildPageLink(basePath, queryString, lastPage),
                Prev = page > 1
                    ? BuildPageLink(basePath, queryString, page - 1)
                    : null,
                Next = page < lastPage
                    ? BuildPageLink(basePath, queryString, page + 1)
                    : null
            },
            Meta = new PaginationMeta
            {
                CurrentPage = page,
                From = from,
                LastPage = lastPage,
                Path = basePath,
                PerPage = perPage,
                To = to,
                Total = total
            }
        };
    }

    public async Task<ProductResponse> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var product = await FindOrFailAsync(id, cancellationToken);

        return ProductResponse.FromEntity(product);
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var sku = NormalizeRequiredString(request.Sku);
        var barcode = NormalizeOptionalString(request.Barcode);

        await ValidateReferencesAsync(
            request.CategoryId,
            request.BrandId,
            request.UnitId,
            cancellationToken);

        await ValidateUniqueAsync(sku, barcode, null, cancellationToken);

        ValidateStockCoherence(
            request.MinimumStock,
            request.MaximumStock,
            request.StockQuantity);

        var product = new Product
        {
            CategoryId = request.CategoryId!.Value,
            BrandId = request.BrandId!.Value,
            UnitId = request.UnitId!.Value,
            Name = NormalizeRequiredString(request.Name),
            Description = NormalizeOptionalString(request.Description),
            Sku = sku,
            Barcode = barcode,
            Price = request.Price!.Value,
            TaxRate = request.TaxRate ?? DefaultTaxRate,
            StockQuantity = request.StockQuantity!.Value,
            MinimumStock = request.MinimumStock ?? DefaultMinimumStock,
            MaximumStock = request.MaximumStock,
            Weight = request.Weight,
            ImageUrl = NormalizeOptionalString(request.ImageUrl),
            IsActive = request.IsActive ?? true
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromEntity(product);
    }

    public async Task<ProductResponse> UpdateAsync(
        string id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await FindOrFailAsync(id, cancellationToken);

        var requestedSku = request.Has("sku")
            ? NormalizeRequiredString(request.Sku)
            : null;
        var requestedBarcode = request.Has("barcode")
            ? NormalizeOptionalString(request.Barcode)
            : null;

        await ValidateReferencesAsync(
            request.Has("category_id") ? request.CategoryId : null,
            request.Has("brand_id") ? request.BrandId : null,
            request.Has("unit_id") ? request.UnitId : null,
            cancellationToken);

        await ValidateUniqueAsync(
            requestedSku,
            requestedBarcode,
            product.Id,
            cancellationToken);

        var minimumStock = request.Has("minimum_stock")
            ? request.MinimumStock
            : product.MinimumStock;
        var maximumStock = request.Has("maximum_stock")
            ? request.MaximumStock
            : product.MaximumStock;
        var stockQuantity = request.Has("stock_quantity")
            ? request.StockQuantity
            : product.StockQuantity;

        ValidateStockCoherence(minimumStock, maximumStock, stockQuantity);

        if (request.Has("category_id"))
        {
            product.CategoryId = request.CategoryId!.Value;
        }

        if (request.Has("brand_id"))
        {
            product.BrandId = request.BrandId!.Value;
        }

        if (request.Has("unit_id"))
        {
            product.UnitId = request.UnitId!.Value;
        }

        if (request.Has("name"))
        {
            product.Name = NormalizeRequiredString(request.Name);
        }

        if (request.Has("description"))
        {
            product.Description = NormalizeOptionalString(request.Description);
        }

        if (request.Has("price"))
        {
            product.Price = request.Price!.Value;
        }

        if (request.Has("sku"))
        {
            product.Sku = requestedSku!;
        }

        if (request.Has("barcode"))
        {
            product.Barcode = requestedBarcode;
        }

        if (request.Has("stock_quantity"))
        {
            product.StockQuantity = request.StockQuantity!.Value;
        }

        if (request.Has("tax_rate"))
        {
            product.TaxRate = request.TaxRate ?? DefaultTaxRate;
        }

        if (request.Has("minimum_stock"))
        {
            product.MinimumStock = request.MinimumStock ?? DefaultMinimumStock;
        }

        if (request.Has("maximum_stock"))
        {
            product.MaximumStock = request.MaximumStock;
        }

        if (request.Has("weight"))
        {
            product.Weight = request.Weight;
        }

        if (request.Has("image_url"))
        {
            product.ImageUrl = NormalizeOptionalString(request.ImageUrl);
        }

        if (request.Has("is_active") && request.IsActive.HasValue)
        {
            product.IsActive = request.IsActive.Value;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromEntity(product);
    }

    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var product = await FindOrFailAsync(id, cancellationToken);

        var hasDependencies = await _db.PurchaseItems
            .AnyAsync(item => item.ProductId == product.Id, cancellationToken);

        if (hasDependencies)
        {
            throw new ProductHasDependenciesException();
        }

        await using var transaction = await _db.Database
            .BeginTransactionAsync(cancellationToken);

        product.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<Product> FindOrFailAsync(
        string id,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(
                id,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var numericId))
        {
            throw new ResourceNotFoundException(ProductNotFoundMessage);
        }

        var product = await _db.Products
            .FirstOrDefaultAsync(candidate => candidate.Id == numericId, cancellationToken);

        return product ?? throw new ResourceNotFoundException(ProductNotFoundMessage);
    }

    private async Task ValidateReferencesAsync(
        int? categoryId,
        int? brandId,
        int? unitId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (categoryId.HasValue
            && !await _db.Categories.AnyAsync(
                category => category.Id == categoryId.Value,
                cancellationToken))
        {
            errors["category_id"] = ["La categoría seleccionada no existe."];
        }

        if (brandId.HasValue
            && !await _db.Brands.AnyAsync(
                brand => brand.Id == brandId.Value,
                cancellationToken))
        {
            errors["brand_id"] = ["La marca seleccionada no existe."];
        }

        if (unitId.HasValue
            && !await _db.Units.AnyAsync(
                unit => unit.Id == unitId.Value,
                cancellationToken))
        {
            errors["unit_id"] = ["La unidad seleccionada no existe."];
        }

        if (errors.Count > 0)
        {
            throw new ApiValidationException(errors);
        }
    }

    private async Task ValidateUniqueAsync(
        string? sku,
        string? barcode,
        int? ignoreId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (!string.IsNullOrEmpty(sku))
        {
            var duplicated = await _db.Products
                .IgnoreQueryFilters()
                .AnyAsync(
                    product => product.Sku == sku
                        && (!ignoreId.HasValue || product.Id != ignoreId.Value),
                    cancellationToken);

            if (duplicated)
            {
                errors["sku"] = ["El SKU ingresado ya está en uso."];
            }
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            var duplicated = await _db.Products
                .IgnoreQueryFilters()
                .AnyAsync(
                    product => product.Barcode == barcode
                        && (!ignoreId.HasValue || product.Id != ignoreId.Value),
                    cancellationToken);

            if (duplicated)
            {
                errors["barcode"] = ["El código de barras ya está en uso."];
            }
        }

        if (errors.Count > 0)
        {
            throw new ApiValidationException(errors);
        }
    }

    private static void ValidateStockCoherence(
        int? minimumStock,
        int? maximumStock,
        int? stockQuantity)
    {
        if (minimumStock.HasValue
            && maximumStock.HasValue
            && minimumStock.Value > maximumStock.Value)
        {
            throw new InvalidProductStockException(
                "El stock mínimo no puede ser mayor que el stock máximo.");
        }

        if (stockQuantity.HasValue
            && minimumStock.HasValue
            && stockQuantity.Value < minimumStock.Value)
        {
            throw new InvalidProductStockException(
                "La cantidad en stock no puede ser menor que el stock mínimo.");
        }

        if (stockQuantity.HasValue
            && maximumStock.HasValue
            && stockQuantity.Value > maximumStock.Value)
        {
            throw new InvalidProductStockException(
                "La cantidad en stock no puede superar el stock máximo.");
        }
    }

    private static string BuildPageLink(
        string path,
        IQueryCollection query,
        int page)
    {
        var parameters = new List<string>();

        foreach (var item in query)
        {
            if (string.Equals(item.Key, "page", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = item.Value.FirstOrDefault() ?? string.Empty;
            parameters.Add(
                $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(value)}");
        }

        parameters.Add($"page={page}");

        return $"{path}?{string.Join("&", parameters)}";
    }

    private static int PositiveInt(string? value, int fallback)
    {
        return int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsed)
            && parsed > 0
                ? parsed
                : fallback;
    }

    private static bool TryParseInt(string? value, out int parsed)
    {
        return int.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out parsed);
    }

    private static bool TryParseDecimal(string? value, out decimal parsed)
    {
        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out parsed);
    }

    private static bool TryParseBoolean(string? value, out bool parsed)
    {
        parsed = false;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        parsed = string.Equals(value, "1", StringComparison.Ordinal)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

        return true;
    }

    private static string NormalizeRequiredString(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string? NormalizeOptionalString(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var trimmed = value.Trim();

        return trimmed.Length == 0 ? null : trimmed;
    }
}
