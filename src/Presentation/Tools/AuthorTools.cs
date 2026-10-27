namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using System.Text.Json;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Authors.Queries;

[McpServerToolType]
public static class AuthorTools
{
    [McpServerTool(Name = "GetAllAuthors", ReadOnly = true)]
    [Description("Get all authors in the system")]
    public static async Task<string> GetAllAuthors(ISender sender)
    {
        var result = await sender.Send(new Queries.GetAuthors.GetAuthorsQuery());

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "GetAuthorById", ReadOnly = true)]
    [Description("Get a specific author by their unique ID")]
    public static async Task<string> GetAuthorById(ISender sender, [Description("The GUID of the author")] Guid id)
    {
        return JsonSerializer.Serialize(await sender.Send(new Queries.GetAuthorById.GetAuthorByIdQuery
        {
            Id = id
        }));
    }
}
