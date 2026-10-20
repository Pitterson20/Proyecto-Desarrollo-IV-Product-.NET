namespace Ferreteria.Auth;

/// <summary>
/// Réplica de RolePermissionSeeder.php: el administrador tiene los 12 recursos
/// completos y los otros roles conservan sus permisos limitados.
/// </summary>
public static class Permissions
{
    public const string ViewProducts = "view products";
    public const string CreateProducts = "create products";
    public const string UpdateProducts = "update products";
    public const string DeleteProducts = "delete products";

    private static readonly string[] Actions = ["view", "create", "update", "delete"];

    private static readonly string[] Resources =
    [
        "products",
        "suppliers",
        "customers",
        "purchases",
        "purchase-items",
        "sales",
        "sale-items",
        "categories",
        "brands",
        "units",
        "inventory-movements",
        "payments"
    ];

    public static IReadOnlyList<string> ProductAbilities { get; } =
    [
        ViewProducts,
        CreateProducts,
        UpdateProducts,
        DeleteProducts
    ];

    public static IReadOnlyList<string> All { get; } =
        Resources
            .SelectMany(resource => Actions.Select(action => $"{action} {resource}"))
            .ToArray();

    public static IReadOnlyList<string> ForRole(Models.UserRole role) => role switch
    {
        Models.UserRole.Admin => All,
        Models.UserRole.Cajero =>
        [
            "view products",
            "view sales",
            "create sales",
            "view customers"
        ],
        Models.UserRole.Bodeguero =>
        [
            "view products",
            "create products",
            "update products",
            "view purchases",
            "view suppliers",
            "create suppliers",
            "update suppliers"
        ],
        _ => []
    };
}
