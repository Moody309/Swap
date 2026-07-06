using Swap.API.Database;
using Swap.API.DTOs;
using Swap.API.Controller;

namespace Swap.API.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto request);
    Task<(bool Success, string Message)> LogoutAsync(LogoutDto dto);
    Task<(bool Success, string Message)> ForgotPasswordAsync(ForgetPasswordDto dto);
    Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto);
}
