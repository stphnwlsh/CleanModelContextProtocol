namespace CleanModelContextProtocol.Presentation.Resources;

using System.ComponentModel;
using CleanModelContextProtocol.Presentation.Models;
using CleanModelContextProtocol.Presentation.Serialization;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Authors.Queries;

[McpServerResourceType]
public static class AuthorResources
{
    [McpServerResource(UriTemplate = "authors://{authorId}", Name = "author", Title = "Author", MimeType = "application/json")]
    [Description("An author with their review count, average stars and every review they have written, including each movie's title.")]
    public static async Task<string> GetAuthor(
        ISender sender,
        [Description("The author's id (a GUID).")] Guid authorId)
    {
        var author = await sender.Send(new Queries.GetAuthorById.GetAuthorByIdQuery
        {
            Id = authorId
        });

        return McpSerializer.Serialize(author.ToResult(includeReviews: true));
    }
}
