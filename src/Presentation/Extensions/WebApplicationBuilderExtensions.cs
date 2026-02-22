namespace CleanModelContextProtocol.Presentation.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application;
using CleanModelContextProtocol.Presentation.Serialization;
using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
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

        _ = builder.Services.Configure<JsonOptions>(opt =>
        {
            opt.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            opt.SerializerOptions.PropertyNameCaseInsensitive = true;
            opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            opt.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        #endregion Serialisation

        #region Validation

        _ = builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

        #endregion Validation

        #region Project Dependencies

        _ = builder.Services.AddInfrastructure();
        _ = builder.Services.AddApplication();

        #endregion Project Dependencies

        #region Model Context Protocol

        _ = builder.Services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new()
                {
                    Name = "Clean Model Context Protocol",
                    Description = "A demonstration of the Clean Model Context Protocol in a .NET application",
                    Title = "Clean Model Context Protocol - Presentation Layer",
                    Version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty,
                    WebsiteUrl = "https://github.com/stphnwlsh/CleanModelContextProtocol",
                    Icons =
                    [
                        new()
                        {
                            Source = "images/Icon.png",
                            MimeType = "image/png"
                        }
                    ],
                };
                options.ServerInstructions = "You are a helpful assistant for handling requests related to movies, " +
                    "authors, and reviews. Use the available tools to retrieve information about movies, authors, and " +
                    "reviews based on the queries you receive. Always ensure that your responses are accurate and relevant " +
                    "to the user's request. Return the full sets of data anc convert them into tabulated markdown formats " +
                    "for display where sensible. Some datasets involve a number of sub objects, please calculate a count of " +
                    "that data for better formatting.";
            })
            .WithHttpTransport()
            .WithToolsFromAssembly();

        #endregion Model Context Protocol

        return builder;
    }
}
