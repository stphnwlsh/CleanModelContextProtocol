namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using System.Text.Json;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Authors.Queries;

[McpServerToolType]
public static class AuthorTools
{
    [McpServerTool(Name = "authors_list", Title = "List authors", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("List every author who has written reviews, with their id, first and last name, and the reviews they have written. " +
        "Use this to find an author's id before calling authors_get, reviews_create or reviews_update.")]
    public static async Task<string> ListAuthors(ISender sender)
    {
        var result = await sender.Send(new Queries.GetAuthors.GetAuthorsQuery());

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "authors_get", Title = "Get author", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Get one author by id, including every review they have written and the movie each review is for.")]
    public static async Task<string> GetAuthor(
        ISender sender,
        [Description("The author's id (a GUID). Use authors_list to find it.")] Guid authorId)
    {
        return JsonSerializer.Serialize(await sender.Send(new Queries.GetAuthorById.GetAuthorByIdQuery
        {
            Id = authorId
        }));
    }
}
