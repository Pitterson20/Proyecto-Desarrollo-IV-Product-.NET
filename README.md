# Proyecto-Desarrollo-IV-Product-.NET

Réplica en ASP.NET Core (.NET 10) de la porción vertical de `Product` construida
en Laravel y NestJS para el Proyecto Integrador de Desarrollo de Software IV.

## Requisitos

- .NET SDK 10.
- SQLite (incluido a través de `Microsoft.EntityFrameworkCore.Sqlite`).

## Ejecución

```powershell
dotnet restore
dotnet run
```

La API queda disponible en `http://localhost:5135` (perfil `http` de
`Properties/launchSettings.json`). Al iniciar se aplican las migraciones y se
siembran los catálogos, los cinco productos y los tres usuarios de prueba.

## Documentación

Swagger UI: `http://localhost:5135/docs/api`

El documento OpenAPI se genera en
`http://localhost:5135/docs/v1/swagger.json`. Para probar endpoints protegidos,
use el botón **Authorize** con el token devuelto por `POST /api/login`.

## Usuarios de prueba

| Correo | Contraseña | Rol |
|-|-|-|
| `admin@example.com` | `password` | admin |
| `cajero@example.com` | `password` | cajero |
| `bodeguero@example.com` | `password` | bodeguero |

## Endpoints

| Método | Ruta | Permiso | Respuesta |
|-|-|-|-|
| `POST` | `/api/login` | público | `200` token JWT; `401` credenciales; `422` datos; `429` límite |
| `POST` | `/api/logout` | autenticado | `200`; revoca el token |
| `GET` | `/api/products` | `view products` | `200` con `data`, `links` y `meta` |
| `GET` | `/api/products/{id}` | `view products` | `200`; `404` si no existe |
| `POST` | `/api/products` | `create products` | `201` + `Location`; `422` validación |
| `PUT` | `/api/products/{id}` | `update products` | `200`; `404`; `409` R2; `422` |
| `DELETE` | `/api/products/{id}` | `delete products` | `204`; `404`; `409` R1 |

Filtros del listado: `page`, `per_page` (máximo 50), `q`, `sort`, `direction`,
`category_id`, `brand_id`, `is_active`, `low_stock`, `min_price`, `max_price`.

Reglas de negocio implementadas:

- **R1:** no se elimina un producto con `purchase_items`; responde `409` con
  `No se puede eliminar el producto porque tiene dependencias activas.`
- **R2:** el stock debe ser coherente con el mínimo y el máximo; responde `409`
  con los mismos mensajes de Laravel/NestJS.
- El borrado es lógico (`deleted_at`) y los productos eliminados no aparecen en
  el listado ni en el detalle.

## Pruebas

```powershell
dotnet test Tests/ProductApi.Tests.csproj
```

Se ejecutan tres pruebas de integración con `WebApplicationFactory` y una base
SQLite temporal por escenario: listado paginado y `401`, creación con `201` y
`Location` más validación `422`, y reglas R1/R2 con autorización `403`.
