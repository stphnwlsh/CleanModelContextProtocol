namespace CleanModelContextProtocol.Presentation.Extensions;

using System.Diagnostics.CodeAnalysis;
#if auth
using CleanModelContextProtocol.Presentation.Authentication;
#endif
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

[ExcludeFromCodeCoverage]
public static class WebApplicationExtensions
{
    public static WebApplication ConfigureApplication(this WebApplication app)
    {
        #region Database

        _ = app.Services.InitialiseInfrastructure(app.Configuration.GetValue<bool>("Database:SeedSampleData"));

        #endregion Database

        #region Security

        // HSTS tells browsers to only use HTTPS from then on, so it's left off for local runs.
        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("local"))
        {
            _ = app.UseHsts();
        }

        _ = app.UseHttpsRedirection();

        #endregion Security

        #region Logging

        _ = app.UseSerilogRequestLogging();

        #endregion Logging

#if auth
        #region Authentication

        if (AuthenticationSettings.FromConfiguration(app.Configuration).Enabled)
        {
            _ = app.UseAuthentication();
        }

        _ = app.UseAuthorization();

        #endregion Authentication

#endif
        #region Health

        // Liveness runs no checks, so it only fails when the process can't answer. Readiness checks the database.
#if auth
        // They aren't behind authorization, so they answer without a token.
#endif
        _ = app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        _ = app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

        #endregion Health

        #region Model Context Protocol

#if auth
        _ = app.MapMcp("/mcp").RequireAuthorization();
#else
        _ = app.MapMcp("/mcp");
#endif

        #endregion Model Context Protocol

        return app;
    }
}
