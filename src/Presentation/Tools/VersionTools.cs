namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Versions.Queries;

[McpServerToolType]
public static class VersionTools
{
    [McpServerTool(Name = "GetVersion", ReadOnly = true)]
    [Description("Get the current application version details")]
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
