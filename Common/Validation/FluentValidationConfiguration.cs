using System.Text.Json;
using FluentValidation;

namespace Ferreteria.Common.Validation;

/// <summary>
/// Hace que los nombres de campo en los errores de FluentValidation coincidan
/// con el contrato JSON (snake_case), sin repetir cadenas en cada regla.
/// </summary>
public static class FluentValidationConfiguration
{
    public static void Configure()
    {
        ValidatorOptions.Global.PropertyNameResolver = (_, memberInfo, expression) =>
        {
            if (memberInfo is not null)
            {
                return JsonNamingPolicy.SnakeCaseLower.ConvertName(memberInfo.Name);
            }

            return expression?.ToString() ?? string.Empty;
        };
    }
}
