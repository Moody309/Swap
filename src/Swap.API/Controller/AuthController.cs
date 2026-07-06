using global::Swap.API.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swap.API.DTOs;
using Swap.API.Entites;
using Swap.API.Services;
using System.Globalization;

namespace Swap.API.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ApplicationDbContext context, ISmsService smsService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
            var userExists = await context.Users
            .AnyAsync(u => u.UserName.ToLower() == request.Username.ToLower());

        if (userExists)
        {
            return BadRequest(new { message = "Username is already taken." });
        }
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(request.Password);
        string hashedPassword = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(passwordBytes));

        var newUser = new Users
        {
            UserName = request.Username,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = hashedPassword
        };

        context.Users.Add(newUser);
        await context.SaveChangesAsync();

        return Ok(new { message = "User registered successfully!" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if (token == null)
        {
            return NotFound("Token not found");
        }

        if (token.IsRevoked)
        {
            return BadRequest("Token already revoked");
        }

        token.IsRevoked = true;
        await context.SaveChangesAsync();

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);

        if (user == null)
        {
            return Ok(new { message = "If the number exists, a code has been sent" });
        }

        var resetCode = new Random().Next(100000, 999999).ToString(CultureInfo.InvariantCulture);

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = resetCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        context.PasswordResetTokens.Add(resetToken);
        await context.SaveChangesAsync();

        await smsService.SendAsync(dto.PhoneNumber, $"Your reset code is: {resetCode}");

        return Ok(new { message = "If the number exists, a code has been sent" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);

        if (user == null)
        {
            return BadRequest("Invalid request");
        }

        var resetToken = await context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.Token == dto.Token)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync();

        if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return BadRequest("Invalid or expired token");
        }

        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(dto.NewPassword);
        string hashedPassword = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(passwordBytes));

        user.PasswordHash = hashedPassword;
        resetToken.IsUsed = true;

        await context.SaveChangesAsync();

        return Ok(new { message = "Password reset successfully" });
    }
}
