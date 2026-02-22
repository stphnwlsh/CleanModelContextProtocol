namespace CleanModelContextProtocol.Presentation.Extensions;

using System.Diagnostics.CodeAnalysis;
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

        _ = app.MapMcp();

        #endregion Model Context Protocol

        return app;
    }
}
