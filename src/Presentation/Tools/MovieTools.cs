namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using CleanModelContextProtocol.Presentation.Models;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Movies.Queries;
using Reviews = Application.Reviews.Queries;

[McpServerToolType]
public static class MovieTools
{
    [McpServerTool(Name = "movies_search", Title = "Search movies", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Find movies whose title contains a search term, ignoring case, in title order. Each result has the movie's id, " +
        "title, review count and average stars. Leave the query empty to browse every movie a page at a time. " +
        "Use this to find a movie's id before calling movies_get, reviews_list or reviews_write.")]
    public static async Task<ResultPage<MovieResult>> SearchMovies(
        ISender sender,
        CancellationToken cancellationToken,
        [Description("Words to look for in the title, such as 'matrix'. Leave empty to list every movie.")] string? query = null,
        [Description("The number of results to skip, 0 for the first page. Pass the nextOffset from a previous call to get the next page.")] int offset = 0,
        [Description("The most results to return, from 1 to 50.")] int limit = 10,
        [Description("Also return every review of each movie. Leave false unless you need the individual reviews.")] bool includeReviews = false)
    {
        var page = await sender.Send(new Queries.GetMovies.GetMoviesQuery
        {
            Query = query,
            Offset = offset,
            Limit = limit
        }, cancellationToken);

        if (!includeReviews || page.Items.Count == 0)
        {
            return page.ToResult(m => m.ToResult());
        }

        // One query gets the reviews of the whole page.
        var reviews = await sender.Send(new Reviews.GetReviewsForMovies.GetReviewsForMoviesQuery
        {
            MovieIds = [.. page.Items.Select(m => m.Id)]
        }, cancellationToken);

        return page.ToResult(m => m.ToResult(reviews[m.Id]));
    }

    [McpServerTool(Name = "movies_get", Title = "Get movie", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Get one movie with its review count, average stars and every review, including each author's name.")]
    public static async Task<MovieResult> GetMovie(
        ISender sender,
        [Description("The movie's id (a GUID). Use movies_search to find it.")] Guid movieId,
        CancellationToken cancellationToken)
    {
        var movie = await sender.Send(new Queries.GetMovieById.GetMovieByIdQuery
        {
            Id = movieId
        }, cancellationToken);

        return movie.ToResult(includeReviews: true);
    }
}
