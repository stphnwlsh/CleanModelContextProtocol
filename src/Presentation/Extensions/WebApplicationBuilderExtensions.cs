namespace CleanModelContextProtocol.Presentation.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Application;
using Application.Common.Telemetry;
using Application.Versions.Entities;
using Infrastructure;
#if includeAuth
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
#endif
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#if includeAuth
using ModelContextProtocol.AspNetCore.Authentication;
#endif
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
#if includeAuth
using Presentation.Authentication;
#endif
using Presentation.Completions;
using Presentation.Errors;
using Presentation.Serialization;
using Presentation.Telemetry;
using Presentation.Tools;
using Serilog;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureApplicationBuilder(this WebApplicationBuilder builder)
    {
        #region Telemetry

        var assembly = typeof(WebApplicationBuilderExtensions).Assembly;
        var telemetry = TelemetrySettings.FromConfiguration(builder.Configuration, builder.Environment.EnvironmentName, assembly);

        var openTelemetry = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddAttributes(telemetry.ResourceAttributes))
            .WithTracing(tracing => tracing
                .AddSource(ApplicationTelemetry.Name, "Experimental.ModelContextProtocol")
                .AddAspNetCoreInstrumentation(options =>
                {
                    // Probes call the health endpoints every few seconds, so tracing them would bury everything else.
                    options.Filter = context => !context.Request.Path.StartsWithSegments("/health");
                    options.RecordException = true;
                })
                .AddHttpClientInstrumentation(options => options.RecordException = true))
            .WithMetrics(metrics => metrics
#if includeAuth
                .AddMeter(
                    "Microsoft.EntityFrameworkCore",
                    "Microsoft.AspNetCore.Authentication",
                    "Microsoft.AspNetCore.Authorization",
                    "Experimental.ModelContextProtocol")
#else
                .AddMeter("Microsoft.EntityFrameworkCore", "Experimental.ModelContextProtocol")
#endif
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .SetExemplarFilter(ExemplarFilterType.TraceBased));

        // The exporters read OTEL_EXPORTER_OTLP_ENDPOINT and the other OTEL_* settings themselves.
        if (telemetry.Enabled)
        {
            _ = openTelemetry.UseOtlpExporter();
        }

        #endregion Telemetry

        #region Logging

        _ = builder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            var configuration = services.GetRequiredService<IConfiguration>();

            _ = loggerConfiguration.ReadFrom.Configuration(configuration)
                .Enrich.WithProperty(
                    "AssemblyVersion",
                    assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version)
                .Enrich.WithProperty(
                    "AssemblyInformationalVersion",
                    assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

            // Logs leave over OTLP too, carrying the trace and span IDs of whatever was running when they were written.
            if (telemetry.Enabled)
            {
                _ = loggerConfiguration.WriteTo.OpenTelemetry(
                    options =>
                    {
                        options.ResourceAttributes = telemetry.ResourceAttributes;
                        options.OnBeginSuppressInstrumentation = SuppressInstrumentationScope.Begin;
                    },
                    key => configuration[key]);
            }
        });

        #endregion Logging

        #region Project Dependencies

        _ = builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        _ = builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("MovieReviews"));
        _ = builder.Services.AddApplication();
        _ = builder.Services.AddSingleton(VersionInfo.FromAssembly(typeof(WebApplicationBuilderExtensions).Assembly));
        _ = builder.Services.AddHealthChecks().AddInfrastructureHealthChecks("ready");

        #endregion Project Dependencies

#if includeAuth
        #region Authentication

        var authentication = AuthenticationSettings.FromConfiguration(builder.Configuration);

        if (authentication.Enabled)
        {
            _ = builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = authentication.Authority;
                    options.Audience = authentication.Audience;
                    options.MapInboundClaims = false;
                })
                .AddMcp(options => options.ResourceMetadata = new()
                {
                    Resource = authentication.Audience,
                    AuthorizationServers = [authentication.Authority!],
                    ScopesSupported = authentication.Scopes,
                    ResourceName = "Clean Model Context Protocol"
                });
            _ = builder.Services.AddAuthorization();
        }
        else
        {
            // The MCP endpoint always asks for authorization, so without an authority every request is let through.
            _ = builder.Services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build());
        }

        #endregion Authentication

#endif
        #region Model Context Protocol

        _ = builder.Services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new()
                {
                    Name = "clean-model-context-protocol",
                    Title = "Clean Model Context Protocol",
                    Description = "Movies, the authors who review them, and their star ratings, served by a Clean Architecture .NET MCP server.",
                    Version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty,
                    // The docs site and a pinned brand release, not this repository: dotnet new renames the
                    // template's name wherever it appears, so repository links break in a new solution.
                    WebsiteUrl = "https://leanmeancleanapps.github.io/mcp/",
                    Icons =
                    [
                        new()
                        {
                            Source = "https://raw.githubusercontent.com/LeanMeanCleanApps/brand/2026.11.04/assets/logos/mcp/png/mcp-square-128.png",
                            MimeType = "image/png"
                        }
                    ],
                };
                options.ServerInstructions = """
                    This server holds a catalogue of movies, the authors who review them, and their reviews.
                    A review links one author to one movie with a star rating from 1 (worst) to 5 (best).
                    Find things by name first: movies_search and authors_search return ids, review counts and average
                    stars, and reviews_list filters reviews by movie or author. Every search returns one page at a time,
                    so pass nextOffset back as the offset to continue rather than raising the limit.
                    reviews_write creates a review, or changes the stars of one when you pass its reviewId. A review's movie and author can't change.
                    reviews_delete can't be undone, so confirm with the user before calling it.
                    Each movie, author and review can also be read as a resource at movies://{id}, authors://{id} or
                    reviews://{id}, and server://version gives this server's version.
                    """;
            })
            .WithHttpTransport(options => options.Stateless = true)
            .WithToolsFromAssembly(serializerOptions: McpSerializer.Options)
            .WithResourcesFromAssembly()
            .WithPromptsFromAssembly(serializerOptions: McpSerializer.Options)
            .WithCompleteHandler(CompletionHandler.Handle)
            .WithRequestFilters(filters =>
            {
                filters.AddListToolsFilter(PortableSchemaFilter.Filter);
                filters.AddCallToolFilter(ToolErrorFilter.Filter);
                filters.AddCallToolFilter(ResourceLinkFilter.Filter);
                filters.AddReadResourceFilter(ProtocolErrorFilter.Filter);
                filters.AddGetPromptFilter(ProtocolErrorFilter.Filter);
            });

        #endregion Model Context Protocol

        return builder;
    }
}
