namespace CleanModelContextProtocol.Presentation.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application;
using CleanModelContextProtocol.Presentation.Errors;
using CleanModelContextProtocol.Presentation.Serialization;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
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

        #region Serialisation

        var serializerOptions = new JsonSerializerOptions(McpJsonUtilities.DefaultOptions)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        serializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);

        #endregion Serialisation

        #region Project Dependencies

        _ = builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        _ = builder.Services.AddInfrastructure();
        _ = builder.Services.AddApplication();

        #endregion Project Dependencies

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
                    """;
            })
            .WithHttpTransport(options => options.Stateless = true)
            .WithToolsFromAssembly(serializerOptions: serializerOptions)
            .WithRequestFilters(filters => filters.AddCallToolFilter(ToolErrorFilter.Filter));

        #endregion Model Context Protocol

        return builder;
    }
}
