using Ferreteria.Auth;
using Ferreteria.DTOs.Common;
using Ferreteria.DTOs.Products;
using Ferreteria.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferreteria.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
[Produces("application/json")]
public sealed class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.ViewProducts)]
    [ProducesResponseType(typeof(PaginatedResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResponse<ProductResponse>>> Index(
        [FromQuery] ProductQuery query,
        CancellationToken cancellationToken)
    {
        var response = await _productService.ListAsync(
            query,
            Request.Path,
            Request.Query,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.ViewProducts)]
    [ProducesResponseType(typeof(DataResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResponse<ProductResponse>>> Show(
        string id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);

        return Ok(new DataResponse<ProductResponse> { Data = product });
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CreateProducts)]
    [ProducesResponseType(typeof(DataResponse<ProductResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<DataResponse<ProductResponse>>> Store(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.CreateAsync(request, cancellationToken);

        Response.Headers.Location = $"/api/products/{product.Id}";

        return StatusCode(
            StatusCodes.Status201Created,
            new DataResponse<ProductResponse> { Data = product });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.UpdateProducts)]
    [ProducesResponseType(typeof(DataResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<DataResponse<ProductResponse>>> Update(
        string id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(new DataResponse<ProductResponse> { Data = product });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.DeleteProducts)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Destroy(
        string id,
        CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
