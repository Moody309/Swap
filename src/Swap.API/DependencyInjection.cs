using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swap.API.Authentication;
using Swap.API.Database;

namespace Swap.API;

/// <summary>
/// Centralized extension methods for registering application services.
/// This keeps Program.cs clean and organized.
/// </summary>

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthenticationInternal();

        builder.Services.AddControllers();

        builder.Services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(t => t.FullName?.Replace("+", "."));
        });

        return builder;
    }
    /// Configures Entity Framework Core with PostgreSQL.
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("Database"),
                npgsqlOptions =>
                    // Store EF migrations history in custom schema
                    npgsqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        Schemas.Swap))
            // Use snake_case naming convention for DB tables/columns
            .UseSnakeCaseNamingConvention());

        return builder;
    }

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
                    .ConfigureResource(resource =>
                        // Register service name in telemetry system
                        resource.AddService(builder.Environment.ApplicationName))
                    .WithTracing(tracing => tracing
                        // Trace outgoing HTTP calls
                        .AddHttpClientInstrumentation()
                        // Trace incoming ASP.NET Core requests
                        .AddAspNetCoreInstrumentation()
                        // Trace PostgreSQL queries
                        .AddNpgsql())
                    .WithMetrics(metrics => metrics
                        // Metrics for outgoing HTTP
                        .AddHttpClientInstrumentation()
                        // Metrics for incoming HTTP
                        .AddAspNetCoreInstrumentation()
                        // Runtime metrics (GC, CPU, etc.)
                        .AddRuntimeInstrumentation())
                    // Export telemetry using OTLP (e.g., to Jaeger, Grafana, etc.)
                    .UseOtlpExporter();

        // Adds OpenTelemetry logging to capture structured logs
        builder.Logging.AddOpenTelemetry(options =>
        {
            // Includes logging scopes (useful for request tracing and correlation IDs)
            options.IncludeScopes = true;

            // Includes the fully formatted log message instead of only template + parameters
            options.IncludeFormattedMessage = true;
        });

        return builder;
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
