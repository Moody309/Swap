namespace Swap.Application.Abstractions.Identity;

/// <summary>
/// Represents the user information required by the identity provider.
/// </summary>
public sealed record UserModel(
    string Email,
    string Password,
    string FirstName,
    string LastName);
