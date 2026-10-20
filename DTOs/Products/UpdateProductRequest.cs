using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ferreteria.DTOs.Products;

/// <summary>
/// Actualización parcial equivalente a las reglas `sometimes` de Laravel.
/// El conversor registra qué campos vino en el JSON para distinguir un campo
/// ausente de un campo enviado explícitamente como null.
/// </summary>
[JsonConverter(typeof(UpdateProductRequestConverter))]
public sealed class UpdateProductRequest
{
    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public int? UnitId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Sku { get; set; }

    public string? Barcode { get; set; }

    public int? StockQuantity { get; set; }

    public decimal? TaxRate { get; set; }

    public int? MinimumStock { get; set; }

    public int? MaximumStock { get; set; }

    public decimal? Weight { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    [JsonIgnore]
    public HashSet<string> ProvidedFields { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonIgnore]
    public Dictionary<string, JsonElement>? Extra { get; set; }

    public bool Has(string field) => ProvidedFields.Contains(field);
}

public sealed class UpdateProductRequestConverter : JsonConverter<UpdateProductRequest>
{
    public override UpdateProductRequest Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("El cuerpo debe ser un objeto JSON.");
        }

        using var document = JsonDocument.ParseValue(ref reader);
        var request = new UpdateProductRequest();

        foreach (var property in document.RootElement.EnumerateObject())
        {
            var name = property.Name;
            request.ProvidedFields.Add(name);

            switch (name)
            {
                case "category_id":
                    request.CategoryId = ReadInt(property.Value);
                    break;
                case "brand_id":
                    request.BrandId = ReadInt(property.Value);
                    break;
                case "unit_id":
                    request.UnitId = ReadInt(property.Value);
                    break;
                case "name":
                    request.Name = ReadString(property.Value);
                    break;
                case "description":
                    request.Description = ReadString(property.Value);
                    break;
                case "price":
                    request.Price = ReadDecimal(property.Value);
                    break;
                case "sku":
                    request.Sku = ReadString(property.Value);
                    break;
                case "barcode":
                    request.Barcode = ReadString(property.Value);
                    break;
                case "stock_quantity":
                    request.StockQuantity = ReadInt(property.Value);
                    break;
                case "tax_rate":
                    request.TaxRate = ReadDecimal(property.Value);
                    break;
                case "minimum_stock":
                    request.MinimumStock = ReadInt(property.Value);
                    break;
                case "maximum_stock":
                    request.MaximumStock = ReadInt(property.Value);
                    break;
                case "weight":
                    request.Weight = ReadDecimal(property.Value);
                    break;
                case "image_url":
                    request.ImageUrl = ReadString(property.Value);
                    break;
                case "is_active":
                    request.IsActive = ReadBool(property.Value);
                    break;
                default:
                    request.Extra ??= new Dictionary<string, JsonElement>();
                    request.Extra[name] = property.Value.Clone();
                    break;
            }
        }

        return request;
    }

    public override void Write(
        Utf8JsonWriter writer,
        UpdateProductRequest value,
        JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    private static int? ReadInt(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.Number when element.TryGetInt32(out var value) => value,
        JsonValueKind.String when int.TryParse(element.GetString(), out var value) => value,
        _ => throw new JsonException("El valor debe ser un número entero.")
    };

    private static decimal? ReadDecimal(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.Number when element.TryGetDecimal(out var value) => value,
        JsonValueKind.String when decimal.TryParse(
            element.GetString(),
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out var value) => value,
        _ => throw new JsonException("El valor debe ser un número.")
    };

    private static bool? ReadBool(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.String when bool.TryParse(element.GetString(), out var value) => value,
        JsonValueKind.Number when element.TryGetInt32(out var value) => value != 0,
        _ => throw new JsonException("El valor debe ser verdadero o falso.")
    };

    private static string? ReadString(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.String => element.GetString(),
        _ => throw new JsonException("El valor debe ser una cadena de texto.")
    };
}
