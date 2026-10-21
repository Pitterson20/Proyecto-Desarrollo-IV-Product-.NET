using System.Text.Json;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ferreteria.Common.Swagger;

/// <summary>
/// Ajusta los esquemas generados para que coincidan con la política JSON
/// global (snake_case) usada por la API.
/// </summary>
public sealed class SnakeCaseSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concreteSchema
            || concreteSchema.Properties is null)
        {
            return;
        }

        concreteSchema.Properties = concreteSchema.Properties.ToDictionary(
            property => JsonNamingPolicy.SnakeCaseLower.ConvertName(property.Key),
            property => property.Value);

        if (concreteSchema.Required is not null)
        {
            concreteSchema.Required = concreteSchema.Required
                .Select(required => JsonNamingPolicy.SnakeCaseLower.ConvertName(required))
                .ToHashSet(StringComparer.Ordinal);
        }
    }
}
