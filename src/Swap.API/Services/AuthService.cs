using Microsoft.EntityFrameworkCore;
using Swap.API.Controller;
using Swap.API.Database;
using Swap.API.DTOs;
using Swap.API.Entites;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Swap.API.Services;

public class AuthService(ApplicationDbContext context, ISmsService smsService) : IAuthService
{
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto request)
    {
        var userExists = await context.Users
            .AnyAsync(u => u.UserName.ToLower() == request.Username.ToLower());

        if (userExists)
        {
            return (false, "Username is already taken.");
        }

        var hashedPassword = HashPassword(request.Password);

        var newUser = new Users
        {
            UserName = request.Username,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = hashedPassword
        };

        context.Users.Add(newUser);
        await context.SaveChangesAsync();

        return (true, "User registered successfully!");
    }

    public async Task<(bool Success, string Message)> LogoutAsync(LogoutDto dto)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if (token == null)
        {
            return (false, "Token not found");
        }

        if (token.IsRevoked)
        {
            return (false, "Token already revoked");
        }

        token.IsRevoked = true;
        await context.SaveChangesAsync();

        return (true, "Logged out successfully");
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgetPasswordDto dto)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);

        const string genericMessage = "If the number exists, a code has been sent";

        if (user == null)
        {
            return (true, genericMessage);
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

        return (true, genericMessage);
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);

        if (user == null)
        {
            return (false, "Invalid request");
        }

        var resetToken = await context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.Token == dto.Token)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync();

        if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return (false, "Invalid or expired token");
        }

        user.PasswordHash = HashPassword(dto.NewPassword);
        resetToken.IsUsed = true;

        await context.SaveChangesAsync();

        return (true, "Password reset successfully");
    }

    private static string HashPassword(string password)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        return Convert.ToBase64String(SHA256.HashData(passwordBytes));
    }
}
