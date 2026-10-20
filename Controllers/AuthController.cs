using Ferreteria.Auth;
using Ferreteria.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferreteria.Controllers;

[ApiController]
[Route("api")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ICurrentUserAccessor _currentUser;

    public AuthController(
        AuthService authService,
        ICurrentUserAccessor currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
        var response = await _authService.LoginAsync(request, ipAddress, cancellationToken);

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult Logout()
    {
        _authService.Logout(_currentUser.User);

        return Ok(new
        {
            message = AuthService.LogoutMessage
        });
    }
}
