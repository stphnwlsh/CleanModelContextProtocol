namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using Mediator;
using ModelContextProtocol.Server;
using Entities = Application.Versions.Entities;
using Queries = Application.Versions.Queries;

[McpServerToolType]
public static class VersionTools
{
    [McpServerTool(Name = "server_get_version", Title = "Get server version", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Get the version of this server: the file version and the informational version, which includes the commit it was built from.")]
    public static async Task<Entities.Version> GetVersion(ISender sender)
    {
        return await sender.Send(new Queries.GetVersion.GetVersionQuery());
    }
}
