using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Swap.Infrastructure.Identity;

internal sealed class KeyCloakAuthDelegatingHandler(
    IOptions<KeyCloakOptions> options) : DelegatingHandler
{
    // Read the Keycloak configuration from the options pattern.
    private readonly KeyCloakOptions _options = options.Value;

    /// <summary>
    /// Adds a bearer token to every outgoing HTTP request.
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Request an access token from Keycloak.
        AuthToken authorizationToken = await GetAuthorizationToken(cancellationToken);

        // Attach the access token as a Bearer token.
        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                authorizationToken.AccessToken);

        // Continue the HTTP pipeline and send the original request.
        HttpResponseMessage httpResponseMessage =
            await base.SendAsync(request, cancellationToken);

        // Throw an exception if the request failed.
        httpResponseMessage.EnsureSuccessStatusCode();

        // Return the successful response.
        return httpResponseMessage;
    }

    /// <summary>
    /// Retrieves an access token using the client credentials flow.
    /// </summary>
    private async Task<AuthToken> GetAuthorizationToken(
        CancellationToken canclationToken)
    {
        // Build the form parameters required by Keycloak's token endpoint.
        var authRequestParameters = new KeyValuePair<string, string>[]
        {
            // The confidential client ID.
            new("client_id", _options.ConfidentialClientId),

            // The confidential client secret.
            new("client_secret", _options.ConfidentialClientSecret),

            // Use the Client Credentials OAuth2 grant.
            new("grant_type", "client_credentials")
        };

        // Create application/x-www-form-urlencoded request content.
        using var authRequestContent =
            new FormUrlEncodedContent(authRequestParameters);

        // Create a POST request to the Keycloak token endpoint.
        using var authRequest = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(_options.TokenUrl));

        // Attach the form data to the request.
        authRequest.Content = authRequestContent;

        // Send the request to Keycloak.
        using HttpResponseMessage authorizationResponse =
            await base.SendAsync(authRequest, canclationToken);

        // Throw an exception if Keycloak returned an error.
        authorizationResponse.EnsureSuccessStatusCode();

        // Deserialize the JSON response into an AuthToken object.
        return await authorizationResponse.Content.ReadFromJsonAsync<AuthToken>(
            canclationToken);
    }

    /// <summary>
    /// Represents the access token returned by Keycloak.
    /// </summary>
    internal sealed class AuthToken
    {
        // Maps the JSON property "access_token" to this C# property.
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; }
    }
}
