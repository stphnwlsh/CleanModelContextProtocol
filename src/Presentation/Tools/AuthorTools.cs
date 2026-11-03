namespace CleanModelContextProtocol.Presentation.Tools;

using CleanModelContextProtocol.Presentation.Models;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Authors.Queries;
using Reviews = Application.Reviews.Queries;

[McpServerToolType]
public static partial class AuthorTools
{
    /// <summary>
    /// Find the authors who write reviews by first name, last name or full name, ignoring case, in last-name order. Each
    /// result has the author's id, name, review count and the average stars they give. Leave the query empty to browse
    /// every author a page at a time. Use this to find an author's id before calling reviews_list or reviews_write.
    /// </summary>
    /// <param name="query">Part of the author's name, such as 'marie' or 'marie hirthe'. Leave empty to list every author.</param>
    /// <param name="offset">The number of results to skip, 0 for the first page. Pass the nextOffset from a previous call to get the next page.</param>
    /// <param name="limit">The most results to return, from 1 to 50.</param>
    /// <param name="includeReviews">Also return every review each author has written. Leave false unless you need the individual reviews.</param>
    [McpServerTool(Name = "authors_search", Title = "Search authors", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    public static async partial Task<ResultPage<AuthorResult>> SearchAuthors(
        ISender sender,
        CancellationToken cancellationToken,
        string? query = null,
        int offset = 0,
        int limit = 10,
        bool includeReviews = false)
    {
        var page = await sender.Send(new Queries.GetAuthors.GetAuthorsQuery
        {
            Query = query,
            Offset = offset,
            Limit = limit
        }, cancellationToken);

        if (!includeReviews || page.Items.Count == 0)
        {
            return page.ToResult(a => a.ToResult());
        }

        // One query gets the reviews of the whole page.
        var reviews = await sender.Send(new Reviews.GetReviewsForAuthors.GetReviewsForAuthorsQuery
        {
            AuthorIds = [.. page.Items.Select(a => a.Id)]
        }, cancellationToken);

        return page.ToResult(a => a.ToResult(reviews[a.Id]));
    }
}
