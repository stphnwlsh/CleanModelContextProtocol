namespace CleanModelContextProtocol.Presentation.Resources;

using CleanModelContextProtocol.Presentation.Models;
using CleanModelContextProtocol.Presentation.Serialization;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Authors.Queries;

[McpServerResourceType]
public static partial class AuthorResources
{
    /// <summary>An author with their review count, average stars and every review they have written, including each movie's title.</summary>
    /// <param name="authorId">The author's id (a GUID).</param>
    [McpServerResource(UriTemplate = "authors://{authorId}", Name = "author", Title = "Author", MimeType = "application/json")]
    public static async partial Task<string> GetAuthor(
        ISender sender,
        Guid authorId,
        CancellationToken cancellationToken)
    {
        var author = await sender.Send(new Queries.GetAuthorById.GetAuthorByIdQuery
        {
            Id = authorId
        }, cancellationToken);

        return McpSerializer.Serialize(author.ToResult(includeReviews: true));
    }
}
