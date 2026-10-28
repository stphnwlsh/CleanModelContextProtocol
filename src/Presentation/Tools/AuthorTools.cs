namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using CleanModelContextProtocol.Presentation.Models;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Authors.Queries;

[McpServerToolType]
public static class AuthorTools
{
    [McpServerTool(Name = "authors_search", Title = "Search authors", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Find the authors who write reviews by first name, last name or full name, ignoring case, in last-name order. " +
        "Each result has the author's id, name, review count and the average stars they give. Leave the query empty to browse " +
        "every author a page at a time. Use this to find an author's id before calling reviews_list or reviews_write.")]
    public static async Task<ResultPage<AuthorResult>> SearchAuthors(
        ISender sender,
        CancellationToken cancellationToken,
        [Description("Part of the author's name, such as 'marie' or 'marie hirthe'. Leave empty to list every author.")] string? query = null,
        [Description("The number of results to skip, 0 for the first page. Pass the nextOffset from a previous call to get the next page.")] int offset = 0,
        [Description("The most results to return, from 1 to 50.")] int limit = 10,
        [Description("Also return every review each author has written. Leave false unless you need the individual reviews.")] bool includeReviews = false)
    {
        var page = await sender.Send(new Queries.GetAuthors.GetAuthorsQuery
        {
            Query = query,
            Offset = offset,
            Limit = limit
        }, cancellationToken);

        return page.ToResult(a => a.ToResult(includeReviews));
    }
}
