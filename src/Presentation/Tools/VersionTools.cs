namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Versions.Queries;

[McpServerToolType]
public static class VersionTools
{
    [McpServerTool(Name = "server_get_version", Title = "Get server version", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Get the version of this server: the file version and the informational version, which includes the commit it was built from.")]
    public static async Task<string> GetVersion(ISender sender)
    {
        var result = await sender.Send(new Queries.GetVersion.GetVersionQuery());

        return $"""
            | Property | Value |
            | --- | --- |
            | File Version | {result.FileVersion} |
            | Informational Version | {result.InformationalVersion} |
            """;
    }
}
