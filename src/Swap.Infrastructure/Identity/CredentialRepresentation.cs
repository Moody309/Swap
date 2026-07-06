namespace Swap.Infrastructure.Identity;

/// <summary>
/// Represents a Keycloak user credential.
/// </summary>
internal sealed record CredentialRepresentation(
    string Type,
    string Value,
    bool Temporary);
