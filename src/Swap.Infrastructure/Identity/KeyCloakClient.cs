using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Swap.Infrastructure.Identity;

// Client responsible for communicating with Keycloak's REST APIs.
internal sealed class KeyCloakClient(
    HttpClient httpClient,
    IOptions<KeyCloakOptions> options)
{
    private readonly KeyCloakOptions _options = options.Value;

    /// <summary>
    /// Authenticates a user against Keycloak's token endpoint using
    /// the Resource Owner Password Credentials grant.
    /// </summary>
    internal async Task<KeyCloakTokenResponse> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        // Build the form data expected by Keycloak's token endpoint.
        var requestParameters = new KeyValuePair<string, string>[]
        {
            new("client_id", _options.PublicClientId),
            new("grant_type", "password"),
            new("username", email),
            new("password", password)
        };

        using var requestContent = new FormUrlEncodedContent(requestParameters);

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl)
        {
            Content = requestContent
        };

        // Send the authentication request.
        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);

        // Throw if authentication failed.
        response.EnsureSuccessStatusCode();

        // Deserialize the issued access and refresh tokens.
        KeyCloakTokenResponse? token = await response.Content
            .ReadFromJsonAsync<KeyCloakTokenResponse>(cancellationToken);

        return token!;
    }

    /// <summary>
    /// Invalidates the supplied refresh token, ending the user's session.
    /// </summary>
    internal async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        // Build the logout request expected by Keycloak.
        var requestParameters = new KeyValuePair<string, string>[]
        {
            new("client_id", _options.PublicClientId),
            new("refresh_token", refreshToken)
        };

        using var requestContent = new FormUrlEncodedContent(requestParameters);

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.LogoutUrl)
        {
            Content = requestContent
        };

        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);

        // Throw if logout failed.
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Creates a new user in Keycloak and returns its identity ID.
    /// </summary>
    internal async Task<string> RegisterUserAsync(
        UserRepresentation user,
        CancellationToken cancellationToken = default)
    {
        // Create the user through the Keycloak Admin API.
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "users",
            user,
            cancellationToken);

        // Ensure the user was created successfully.
        response.EnsureSuccessStatusCode();

        // Extract the generated identity ID from the Location header.
        return ExtractIdentityIdFromLocationHeader(response);
    }

    /// <summary>
    /// Extracts the generated Keycloak user ID from the Location response header.
    /// </summary>
    private static string ExtractIdentityIdFromLocationHeader(
        HttpResponseMessage httpResponseMessage)
    {
        // Keycloak returns the created user's URL in the Location header.
        const string userSegmentName = "users/";

        // Read the Location header.
        string? locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;

        // A successful user creation should always include the Location header.
        if (locationHeader is null)
        {
            throw new InvalidOperationException("Location header is null");
        }

        // Locate the "users/" segment.
        int userSegmentValueIndex = locationHeader.IndexOf(
            userSegmentName,
            StringComparison.InvariantCultureIgnoreCase);

        // Everything after "users/" is the Keycloak identity ID.
        string identityId = locationHeader.Substring(
            userSegmentValueIndex + userSegmentName.Length);

        return identityId;
    }

    // Represents the token payload returned by Keycloak.
    internal sealed class KeyCloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; init; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }
}
