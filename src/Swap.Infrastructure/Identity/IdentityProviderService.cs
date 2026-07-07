using System.Net;
using Microsoft.Extensions.Logging;
using Swap.Application.Abstractions.Identity;
using Swap.Domain.Results;

namespace Swap.Infrastructure.Identity;

// Implements the application's identity operations using Keycloak.
internal sealed class IdentityProviderService(
    KeyCloakClient keyCloakClient,
    ILogger<IdentityProviderService> logger) : IIdentityProviderService
{
    /// <summary>
    /// Keycloak password credential type.
    /// </summary>
    private const string PasswordCredentialType = "password";

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    public async Task<Result<TokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Delegate authentication to Keycloak.
            KeyCloakClient.KeyCloakTokenResponse token =
                await keyCloakClient.LoginAsync(email, password, cancellationToken);

            return new TokenResponse(
                token.AccessToken,
                token.RefreshToken,
                token.ExpiresIn);
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Translate authentication failures into a domain-specific error.
            logger.LogWarning(exception, "Failed login attempt for {Email}", email);

            return Result.Failure<TokenResponse>(
                IdentityProviderErrors.InvalidCredentials);
        }
    }

    /// <summary>
    /// Logs the user out by invalidating the refresh token.
    /// </summary>
    public async Task<Result> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await keyCloakClient.LogoutAsync(refreshToken, cancellationToken);

            return Result.Success();
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode == HttpStatusCode.BadRequest)
        {
            // The refresh token is already invalid, so the session is effectively ended.
            logger.LogInformation(
                exception,
                "Logout called with an already-invalid refresh token");

            return Result.Success();
        }
    }

    // POST /admin/realms/{realm}/users
    // https://www.keycloak.org/docs-api/latest/rest-api/index.html#_post_adminrealmsrealmusers

    /// <summary>
    /// Creates a new identity in Keycloak.
    /// </summary>
    public async Task<Result<string>> RegisterUserAsync(
        UserModel user,
        CancellationToken cancellationToken = default)
    {
        // Map the application user model to Keycloak's representation.
        var userRepresentation = new UserRepresentation(
            user.Email,
            user.Email,
            user.FirstName,
            user.LastName,
            true,
            true,
            [
                new CredentialRepresentation(
                    PasswordCredentialType,
                    user.Password,
                    false)
            ]);

        try
        {
            string identityId = await keyCloakClient.RegisterUserAsync(
                userRepresentation,
                cancellationToken);

            return identityId;
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            // Translate duplicate email errors into a domain-specific result.
            logger.LogError(
                exception,
                "Failed to register user {Email} in identity provider",
                user.Email);

            return Result.Failure<string>(
                IdentityProviderErrors.EmailIsNotUnique);
        }
    }
}
