namespace Swap.Infrastructure.Identity;

/// <summary>
/// Represents the user payload sent to Keycloak.
/// </summary>
internal sealed record UserRepresentation(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool EmailVerified,
    bool Enabled,
    CredentialRepresentation[] Credentials);
