namespace Swap.Application.Abstractions.Identity;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);
