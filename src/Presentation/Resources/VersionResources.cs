namespace CleanModelContextProtocol.Presentation.Resources;

using CleanModelContextProtocol.Presentation.Serialization;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Versions.Queries;

[McpServerResourceType]
public static partial class VersionResources
{
    /// <summary>The version of this server: the file version and the informational version, which includes the commit it was built from.</summary>
    [McpServerResource(UriTemplate = "server://version", Name = "server_version", Title = "Server version", MimeType = "application/json")]
    public static async partial Task<string> GetVersion(ISender sender, CancellationToken cancellationToken)
    {
        return McpSerializer.Serialize(await sender.Send(new Queries.GetVersion.GetVersionQuery(), cancellationToken));
    }
}
