using System.Net;
using Microsoft.Extensions.Logging;
using Swap.Application.Abstractions.Identity;
using Swap.Domain.Results;

namespace Swap.Infrastructure.Identity;

internal sealed class IdentityProviderService(
    KeyCloakClient keyCloakClient,
    ILogger<IdentityProviderService> logger) : IIdentityProviderService
{
    /// <summary>
    /// Keycloak password credential type.
    /// </summary>
    private const string PasswordCredentialType = "password";

    // POST /admin/realms/{realm}/users
    // https://www.keycloak.org/docs-api/latest/rest-api/index.html#_post_adminrealmsrealmusers
    public async Task<Result<string>> RegisterUserAsync(
        UserModel user,
        CancellationToken cancellationToken = default)
    {
        // Map the application user to Keycloak's user representation.
        var userRepresentation = new UserRepresentation(
            user.Email,
            user.Email,
            user.FirstName,
            user.LastName,
            true,
            true,
            [new CredentialRepresentation(PasswordCredentialType, user.Password, false)]);

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
            // Email already exists in Keycloak.
            logger.LogError(
                exception,
                "Failed to register user {Email} in identity provider",
                user.Email);

            return Result.Failure<string>(IdentityProviderErrors.EmailIsNotUnique);
        }
    }
}
