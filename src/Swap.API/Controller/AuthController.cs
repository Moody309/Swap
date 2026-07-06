
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swap.API.Database;
using Swap.API.DTOs;
using Swap.API.Services;

namespace Swap.API.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var (success, message) = await authService.RegisterAsync(request);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
    {
        var (success, message) = await authService.LogoutAsync(dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto dto)
    {
        var (_, message) = await authService.ForgotPasswordAsync(dto);
        return Ok(new { message });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var (success, message) = await authService.ResetPasswordAsync(dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}
