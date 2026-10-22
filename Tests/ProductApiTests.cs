using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ferreteria.Data;
using Ferreteria.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProductApi.Tests;

public sealed class ProductApiTests
{
    [Fact]
    public async Task ListadoPaginadoDevuelveDataLinksMetaYExigeToken()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();

        var token = await LoginAsync(client, "cajero@example.com");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/api/products?per_page=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        var root = body.RootElement;

        Assert.Equal(2, root.GetProperty("data").GetArrayLength());
        Assert.Equal(5, root.GetProperty("meta").GetProperty("total").GetInt32());
        Assert.Equal(1, root.GetProperty("meta").GetProperty("current_page").GetInt32());
        Assert.True(root.GetProperty("links").TryGetProperty("first", out _));
        Assert.True(root.GetProperty("links").TryGetProperty("last", out _));
        Assert.True(
            root.GetProperty("data")[0].TryGetProperty("nombre", out _));
        Assert.True(
            root.GetProperty("data")[0].TryGetProperty("codigo_barras", out _));

        using var anonymousClient = factory.CreateClient();
        using var unauthorized = await anonymousClient.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
    }

    [Fact]
    public async Task CrearProductoDevuelve201LocationYRechazaDatosInvalidos()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();

        var token = await LoginAsync(client, "admin@example.com");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var created = await client.PostAsJsonAsync("/api/products", new
        {
            category_id = 1,
            brand_id = 1,
            unit_id = 1,
            name = "Martillo 16oz",
            description = "Martillo de acero con mango de fibra",
            price = 12500.0,
            sku = "SKU-NEW-001",
            barcode = "7509999999999",
            stock_quantity = 45,
            minimum_stock = 10,
            maximum_stock = 100,
            tax_rate = 0.13,
            weight = 0.45,
            is_active = true
        });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Matches(
            "^/api/products/\\d+$",
            created.Headers.Location?.ToString() ?? string.Empty);

        using var createdBody = JsonDocument.Parse(
            await created.Content.ReadAsStringAsync());
        var createdProduct = createdBody.RootElement.GetProperty("data");

        Assert.Equal("Martillo 16oz", createdProduct.GetProperty("nombre").GetString());
        Assert.Equal(
            "7509999999999",
            createdProduct.GetProperty("codigo_barras").GetString());

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();
        var countBefore = await db.Products.CountAsync();

        using var invalid = await client.PostAsJsonAsync("/api/products", new
        {
            category_id = 1,
            brand_id = 1,
            unit_id = 1,
            name = new string('x', 151),
            price = -5,
            sku = "SKU-BAD-001",
            barcode = new string('y', 51),
            stock_quantity = 1,
            image_url = "no-es-una-url"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalid.StatusCode);

        using var invalidBody = JsonDocument.Parse(
            await invalid.Content.ReadAsStringAsync());
        var invalidRoot = invalidBody.RootElement;

        Assert.Equal(
            "Los datos enviados no son válidos.",
            invalidRoot.GetProperty("message").GetString());

        var errors = invalidRoot.GetProperty("errors");
        Assert.True(errors.TryGetProperty("name", out _));
        Assert.True(errors.TryGetProperty("price", out _));
        Assert.True(errors.TryGetProperty("barcode", out _));
        Assert.True(errors.TryGetProperty("image_url", out _));
        Assert.Equal(countBefore, await db.Products.CountAsync());
    }

    [Fact]
    public async Task ReglasDeNegocioYAutorizacionFuncionan()
    {
        await using var factory = new ProductApiFactory();
        using var admin = factory.CreateClient();
        using var cajero = factory.CreateClient();

        var adminToken = await LoginAsync(admin, "admin@example.com");
        admin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var cajeroToken = await LoginAsync(cajero, "cajero@example.com");
        cajero.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", cajeroToken);

        using var created = await admin.PostAsJsonAsync("/api/products", new
        {
            category_id = 1,
            brand_id = 1,
            unit_id = 1,
            name = "Producto con dependencias",
            price = 100,
            sku = "SKU-R1-001",
            stock_quantity = 10,
            minimum_stock = 1,
            maximum_stock = 50
        });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        using var createdBody = JsonDocument.Parse(
            await created.Content.ReadAsStringAsync());
        var productId = createdBody.RootElement
            .GetProperty("data")
            .GetProperty("id")
            .GetInt32();

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();
            db.PurchaseItems.Add(new PurchaseItem
            {
                ProductId = productId,
                Quantity = 1
            });
            await db.SaveChangesAsync();
        }

        using var dependency = await admin.DeleteAsync($"/api/products/{productId}");
        Assert.Equal(HttpStatusCode.Conflict, dependency.StatusCode);

        using var dependencyBody = JsonDocument.Parse(
            await dependency.Content.ReadAsStringAsync());
        Assert.Equal(
            "No se puede eliminar el producto porque tiene dependencias activas.",
            dependencyBody.RootElement.GetProperty("message").GetString());

        using var badStock = await admin.PostAsJsonAsync("/api/products", new
        {
            category_id = 1,
            brand_id = 1,
            unit_id = 1,
            name = "Stock incoherente",
            price = 100,
            sku = "SKU-R2-001",
            stock_quantity = 5,
            minimum_stock = 100,
            maximum_stock = 10
        });

        Assert.Equal(HttpStatusCode.Conflict, badStock.StatusCode);

        using var badStockBody = JsonDocument.Parse(
            await badStock.Content.ReadAsStringAsync());
        Assert.Equal(
            "El stock mínimo no puede ser mayor que el stock máximo.",
            badStockBody.RootElement.GetProperty("message").GetString());

        using var badUpdate = await admin.PutAsJsonAsync(
            $"/api/products/{productId}",
            new { stock_quantity = 999999 });
        Assert.Equal(HttpStatusCode.Conflict, badUpdate.StatusCode);

        using var forbidden = await cajero.DeleteAsync("/api/products/2");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using var stillThere = await admin.GetAsync("/api/products/2");
        Assert.Equal(HttpStatusCode.OK, stillThere.StatusCode);

        using var stillThereBody = JsonDocument.Parse(
            await stillThere.Content.ReadAsStringAsync());
        Assert.Equal(
            2,
            stillThereBody.RootElement
                .GetProperty("data")
                .GetProperty("id")
                .GetInt32());
    }

    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        using var response = await client.PostAsJsonAsync("/api/login", new
        {
            email,
            password = "password"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        return body.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("La respuesta no incluyó token.");
    }
}
