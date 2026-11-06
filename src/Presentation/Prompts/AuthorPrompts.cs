namespace CleanModelContextProtocol.Presentation.Prompts;

using Mediator;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Presentation.Models;
using Presentation.Serialization;
using Presentation.Tools;
using Queries = Application.Authors.Queries;

[McpServerPromptType]
public static partial class AuthorPrompts
{
    /// <summary>Summarise an author's reviewing habits: how many reviews they've written, how generous they are, and the movies they rated highest and lowest.</summary>
    /// <param name="authorName">The author's name, or part of it.</param>
    [McpServerPrompt(Name = "summarise_author_reviews", Title = "Summarise an author's reviews", IconSource = ItemIcons.Author)]
    public static async partial Task<GetPromptResult> SummariseAuthorReviews(
        ISender sender,
        string authorName,
        CancellationToken cancellationToken)
    {
        var author = await FindAuthor(sender, authorName, cancellationToken);

        var messages = new List<PromptMessage>
        {
            PromptContent.Text(author is null
                ? $"Summarise the reviewing habits of the author \"{authorName}\". Find them with authors_search, get their reviews " +
                    "with reviews_list, then say how many reviews they've written, how generous they are, and which movies they rated " +
                    "highest and lowest."
                : $"Summarise the reviewing habits of {author.Name}, using the author below. Say how many reviews they've written, " +
                    "how generous they are, and which movies they rated highest and lowest.")
        };

        if (author is not null)
        {
            messages.Add(PromptContent.Embed($"authors://{author.Id}", McpSerializer.Serialize(author)));
        }

        return new GetPromptResult
        {
            Description = $"Summarise the reviews of {author?.Name ?? authorName}",
            Messages = messages
        };
    }

    private static async Task<AuthorResult?> FindAuthor(ISender sender, string? name, CancellationToken cancellationToken)
    {
        var page = await sender.Send(new Queries.GetAuthors.GetAuthorsQuery
        {
            Query = name,
            Limit = 5
        }, cancellationToken);

        var author = page.Items.FirstOrDefault(a => string.Equals(a.ToResult().Name, name?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? (page.TotalCount == 1 ? page.Items[0] : null);

        if (author is null)
        {
            return null;
        }

        // The search only has the review count and average, so get the author again for their reviews.
        var withReviews = await sender.Send(new Queries.GetAuthorById.GetAuthorByIdQuery { Id = author.Id }, cancellationToken);

        return withReviews.ToResult(includeReviews: true);
    }
}
