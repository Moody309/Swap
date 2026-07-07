using Microsoft.AspNetCore.Mvc;
using Swap.Application.Abstractions.Identity;
using Swap.Domain.Results;

namespace Swap.API.Controllers;

[Route("auth")]
[ApiController]
public class AuthController(
    IIdentityProviderService identityProviderService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
    LoginRequest request,
    CancellationToken cancellationToken)
    {
        Result<TokenResponse> result = await identityProviderService.LoginAsync(
            request.Email, request.Password, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(result.Error);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
    LogoutRequest request,
    CancellationToken cancellationToken)
    {
        Result result = await identityProviderService.LogoutAsync(
            request.RefreshToken, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }
}
