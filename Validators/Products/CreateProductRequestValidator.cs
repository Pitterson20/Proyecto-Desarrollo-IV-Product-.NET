using FluentValidation;
using Ferreteria.DTOs.Products;

namespace Ferreteria.Validators.Products;

/// <summary>
/// Réplica de StoreProductRequest.php, incluidos los mensajes en español.
/// </summary>
public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(request => request.CategoryId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("El campo categoría es obligatorio.");

        RuleFor(request => request.BrandId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("El campo marca es obligatorio.");

        RuleFor(request => request.UnitId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("El campo unidad es obligatorio.");

        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(150)
                .WithMessage("El campo nombre no debe exceder los 150 caracteres.");

        RuleFor(request => request.Description)
            .MaximumLength(1000)
            .WithMessage("La descripción no debe exceder los 1000 caracteres.")
            .When(request => !string.IsNullOrEmpty(request.Description));

        RuleFor(request => request.Price)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("El campo precio es obligatorio.")
            .GreaterThanOrEqualTo(0).WithMessage("El campo precio debe ser mayor o igual a 0.")
            .LessThanOrEqualTo(99999999.99m).WithMessage("El campo precio excede el máximo permitido.");

        RuleFor(request => request.Sku)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("El campo SKU es obligatorio.")
            .MaximumLength(50)
                .WithMessage("El campo SKU no debe exceder los 50 caracteres.");

        RuleFor(request => request.Barcode)
            .MaximumLength(50)
            .WithMessage("El código de barras no debe exceder los 50 caracteres.")
            .When(request => !string.IsNullOrEmpty(request.Barcode));

        RuleFor(request => request.StockQuantity)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("El campo cantidad en stock es obligatorio.")
            .GreaterThanOrEqualTo(0)
                .WithMessage("El campo cantidad en stock debe ser mayor o igual a 0.")
            .LessThanOrEqualTo(1000000)
                .WithMessage("El campo cantidad en stock excede el máximo permitido.");

        RuleFor(request => request.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("La tasa de impuesto debe ser mayor o igual a 0.")
            .LessThanOrEqualTo(100).WithMessage("La tasa de impuesto no debe exceder 100.")
            .When(request => request.TaxRate.HasValue);

        RuleFor(request => request.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo debe ser mayor o igual a 0.")
            .LessThanOrEqualTo(1000000).WithMessage("El stock mínimo excede el máximo permitido.")
            .When(request => request.MinimumStock.HasValue);

        RuleFor(request => request.MaximumStock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock máximo debe ser mayor o igual a 0.")
            .LessThanOrEqualTo(1000000).WithMessage("El stock máximo excede el máximo permitido.")
            .When(request => request.MaximumStock.HasValue);

        RuleFor(request => request.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("El peso debe ser mayor o igual a 0.")
            .LessThanOrEqualTo(100000).WithMessage("El peso excede el máximo permitido.")
            .When(request => request.Weight.HasValue);

        RuleFor(request => request.ImageUrl)
            .MaximumLength(255)
            .WithMessage("La URL de la imagen no debe exceder los 255 caracteres.")
            .Must(ProductValidation.IsValidImageUrl)
            .WithMessage("La URL de la imagen debe ser una URL válida.")
            .When(request => !string.IsNullOrEmpty(request.ImageUrl));

        RuleFor(request => request.Extra)
            .Custom((extra, context) =>
            {
                if (extra is null)
                {
                    return;
                }

                foreach (var field in extra.Keys)
                {
                    context.AddFailure(field, $"El campo {field} no está permitido.");
                }
            });
    }
}
