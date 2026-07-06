using System.Net.Http.Json;

namespace Swap.Infrastructure.Identity;

internal sealed class KeyCloakClient(HttpClient httpClient)
{
    /// <summary>
    /// Creates a new user in Keycloak and returns its identity ID.
    /// </summary>
    internal async Task<string> RegisterUserAsync(
        UserRepresentation user,
        CancellationToken cancellationToken = default)
    {
        // Send a POST request to Keycloak to create a new user.
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "users",
            user,
            cancellationToken);

        // Throw an exception if the request failed.
        response.EnsureSuccessStatusCode();

        // Return the user ID from the Location header.
        return ExtractIdentityIdFromLocationHeader(response);
    }

    /// <summary>
    /// Extracts the user ID from the Location response header.
    /// </summary>
    private static string ExtractIdentityIdFromLocationHeader(
        HttpResponseMessage httpResponseMessage)
    {
        // The Location header contains ".../users/{id}".
        const string userSegmentName = "users/";

        // Read the Location header returned by Keycloak.
        string? locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;

        // Creating a user should always return a Location header.
        if (locationHeader is null)
        {
            throw new InvalidOperationException("Location header is null");
        }

        // Find where the "users/" segment starts.
        int userSegmentValueIndex = locationHeader.IndexOf(
            userSegmentName,
            StringComparison.InvariantCultureIgnoreCase);

        // Extract everything after "users/", which is the identity ID.
        string identityId = locationHeader.Substring(
            userSegmentValueIndex + userSegmentName.Length);

        return identityId;
    }
}
