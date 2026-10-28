namespace CleanModelContextProtocol.Presentation.Resources;

using System.ComponentModel;
using CleanModelContextProtocol.Presentation.Serialization;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Versions.Queries;

[McpServerResourceType]
public static class VersionResources
{
    [McpServerResource(UriTemplate = "server://version", Name = "server_version", Title = "Server version", MimeType = "application/json")]
    [Description("The version of this server: the file version and the informational version, which includes the commit it was built from.")]
    public static async Task<string> GetVersion(ISender sender, CancellationToken cancellationToken)
    {
        return McpSerializer.Serialize(await sender.Send(new Queries.GetVersion.GetVersionQuery(), cancellationToken));
    }
}
