namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using Queries = Application.Versions.Queries;

[McpServerToolType]
public static class VersionTools
{
    [McpServerTool(Name = "GetVersion", ReadOnly = true)]
    [Description("Get the current application version details")]
    public static async Task<string> GetVersion(ISender sender)
    {
        try
        {
            var result = await sender.Send(new Queries.GetVersion.GetVersionQuery());

            return $"""
                | Property | Value |
                | --- | --- |
                | File Version | {result.FileVersion} |
                | Informational Version | {result.InformationalVersion} |
                """;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
