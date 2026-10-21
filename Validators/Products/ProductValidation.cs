namespace Ferreteria.Validators.Products;

/// <summary>
/// Reglas compartidas por los validadores de creación y actualización.
/// </summary>
internal static class ProductValidation
{
    public static bool IsValidImageUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && !string.IsNullOrWhiteSpace(uri.Host);
    }
}
