using FluentValidation;
using Ferreteria.DTOs.Auth;

namespace Ferreteria.Validators.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico debe tener un formato válido.")
            .MaximumLength(255).WithMessage("El correo electrónico no debe exceder los 255 caracteres.");

        RuleFor(request => request.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");

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
