using System.Text.Json;
using Ferreteria.Common.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;

namespace Ferreteria.Common.Validation;

/// <summary>
/// Ejecuta los validadores de FluentValidation registrados para cada argumento
/// de la acción y devuelve el mismo 422 que Laravel/NestJS.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            var validator = context.HttpContext.RequestServices.GetService(validatorType);

            if (validator is not IValidator fluentValidator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await fluentValidator.ValidateAsync(validationContext);

            if (result.IsValid)
            {
                continue;
            }

            var errors = result.Errors
                .GroupBy(failure => string.IsNullOrWhiteSpace(failure.PropertyName)
                    ? "body"
                    : failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());

            throw new ApiValidationException(errors);
        }

        await next();
    }
}
