using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Swap.Infrastructure.Authentication;
/// <summary>
/// Configures <see cref="JwtBearerOptions"/> by binding values
/// from the "Authentication" section in the application configuration.
/// </summary>
internal sealed class JwtBearerConfigureOptions(IConfiguration configuration)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    /// <summary>
    /// Name of the configuration section containing JWT authentication settings.
    /// </summary>
    private const string ConfigurationSectionName = "Authentication";

    /// <summary>
    /// Configures the default JWT Bearer options.
    /// </summary>
    public void Configure(JwtBearerOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }

    /// <summary>
    /// Configures JWT Bearer options for a named authentication scheme.
    /// The scheme name is ignored because the same configuration is applied
    /// to all JWT Bearer schemes.
    /// </summary>
    public void Configure(
        string? name,
        JwtBearerOptions options)
    {
        Configure(options);
    }
}
