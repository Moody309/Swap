using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swap.Application.Abstractions.Data;
using Swap.Application.Abstractions.Identity;
using Swap.Domain.Entities.Users;
using Swap.Infrastructure.Authentication;
using Swap.Infrastructure.Database;
using Swap.Infrastructure.Identity;
using Swap.Infrastructure.Repositories;

namespace Swap.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        // Bind Keycloak settings from configuration.
        services.Configure<KeyCloakOptions>(
            configuration.GetSection("KeyCloak"));

        // Handler that attaches an admin access token to Keycloak requests.
        services.AddTransient<KeyCloakAuthDelegatingHandler>();

        // Configure the HTTP client used to communicate with the Keycloak Admin API.
        services
            .AddHttpClient<KeyCloakClient>((serviceProvider, httpClient) =>
            {
                KeyCloakOptions keyCloakOptions = serviceProvider
                    .GetRequiredService<IOptions<KeyCloakOptions>>()
                    .Value;

                // Set the Keycloak Admin API base address.
                httpClient.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
            })
            // Add the authentication handler to outgoing requests.
            .AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();

        // Register the service responsible for identity provider operations.
        services.AddTransient<IIdentityProviderService, IdentityProviderService>();

        services.AddDatabase(configuration);

        // Register repositories.
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRegistrationService, UserRegistrationService>();

        // Register the Unit of Work implementation.
        services.AddScoped<IUnitOfWork>(
            sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    /// Configures Entity Framework Core with PostgreSQL.
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        Schemas.Swap))
            .UseSnakeCaseNamingConvention());

        return services;
    }
    /// <summary>
    /// Registers the application's authentication and authorization services.
    /// This includes:
    /// - Authorization services.
    /// - JWT Bearer authentication.
    /// - HttpContext accessor.
    /// - Configuration binding for <see cref="JwtBearerOptions"/>.
    /// </summary>
    internal static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
    {
        // Register ASP.NET Core authorization services.
        services.AddAuthorization();

        // Register JWT Bearer authentication.
        // The options are configured separately by JwtBearerConfigureOptions.
        services.AddAuthentication().AddJwtBearer();

        // Allows services to access the current HttpContext.
        services.AddHttpContextAccessor();

        // Bind JwtBearerOptions from configuration.
        services.ConfigureOptions<JwtBearerConfigureOptions>();

        return services;
    }
}
