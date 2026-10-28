namespace CleanModelContextProtocol.Presentation.Extensions;

using System.Diagnostics.CodeAnalysis;
using CleanModelContextProtocol.Presentation.Authentication;
using Microsoft.AspNetCore.Builder;
using Serilog;

[ExcludeFromCodeCoverage]
public static class WebApplicationExtensions
{
    public static WebApplication ConfigureApplication(this WebApplication app)
    {
        #region Logging

        _ = app.UseSerilogRequestLogging();

        #endregion Logging

        #region Model Context Protocol

        var mcp = app.MapMcp("/mcp");

        if (AuthenticationSettings.FromConfiguration(app.Configuration).Enabled)
        {
            _ = app.UseAuthentication();
            _ = app.UseAuthorization();
            _ = mcp.RequireAuthorization();
        }

        #endregion Model Context Protocol

        return app;
    }
}
