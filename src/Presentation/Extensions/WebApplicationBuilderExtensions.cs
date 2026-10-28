namespace CleanModelContextProtocol.Presentation.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Application;
using CleanModelContextProtocol.Presentation.Authentication;
using CleanModelContextProtocol.Presentation.Completions;
using CleanModelContextProtocol.Presentation.Errors;
using CleanModelContextProtocol.Presentation.Serialization;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore.Authentication;
using Serilog;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureApplicationBuilder(this WebApplicationBuilder builder)
    {
        var assembly = Assembly.GetEntryAssembly();

        #region Logging

        _ = builder.Host.UseSerilog((hostContext, loggerConfiguration) => _ = loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration)
                .Enrich.WithProperty(
                    "Assembly Version",
                    assembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version)
                .Enrich.WithProperty(
                    "Assembly Informational Version",
                    assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion));

        #endregion Logging

        #region Project Dependencies

        _ = builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        _ = builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("MovieReviews"));
        _ = builder.Services.AddApplication();

        #endregion Project Dependencies

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

        #endregion Authentication

        #region Model Context Protocol

        _ = builder.Services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new()
                {
                    Name = "clean-model-context-protocol",
                    Title = "Clean Model Context Protocol",
                    Description = "Movies, the authors who review them, and their star ratings, served by a Clean Architecture .NET MCP server.",
                    Version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty,
                    WebsiteUrl = "https://github.com/stphnwlsh/CleanModelContextProtocol",
                    Icons =
                    [
                        new()
                        {
                            Source = "https://raw.githubusercontent.com/stphnwlsh/CleanModelContextProtocol/main/icon.png",
                            MimeType = "image/png"
                        }
                    ],
                };
                options.ServerInstructions = """
                    This server holds a catalogue of movies, the authors who review them, and their reviews.
                    A review links one author to one movie with a star rating from 1 (worst) to 5 (best).
                    Find things by name first: movies_search and authors_search return ids, review counts and average
                    stars, and reviews_list filters reviews by movie or author. Every search returns one page at a time,
                    so pass nextCursor back to continue rather than raising the limit.
                    reviews_write creates a review, or replaces one when you pass its reviewId.
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
                filters.AddCallToolFilter(ToolErrorFilter.Filter);
                filters.AddReadResourceFilter(ProtocolErrorFilter.Filter);
                filters.AddGetPromptFilter(ProtocolErrorFilter.Filter);
            });

        #endregion Model Context Protocol

        return builder;
    }
}
