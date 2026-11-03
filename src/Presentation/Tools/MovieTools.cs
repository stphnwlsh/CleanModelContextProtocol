namespace CleanModelContextProtocol.Presentation.Tools;

using CleanModelContextProtocol.Presentation.Models;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Movies.Queries;
using Reviews = Application.Reviews.Queries;

[McpServerToolType]
public static partial class MovieTools
{
    /// <summary>
    /// Find movies whose title contains a search term, ignoring case, in title order. Each result has the movie's id,
    /// title, review count and average stars. Leave the query empty to browse every movie a page at a time. Use this to
    /// find a movie's id before calling movies_get, reviews_list or reviews_write.
    /// </summary>
    /// <param name="query">Words to look for in the title, such as 'matrix'. Leave empty to list every movie.</param>
    /// <param name="offset">The number of results to skip, 0 for the first page. Pass the nextOffset from a previous call to get the next page.</param>
    /// <param name="limit">The most results to return, from 1 to 50.</param>
    /// <param name="includeReviews">Also return every review of each movie. Leave false unless you need the individual reviews.</param>
    [McpServerTool(Name = "movies_search", Title = "Search movies", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    public static async partial Task<ResultPage<MovieResult>> SearchMovies(
        ISender sender,
        CancellationToken cancellationToken,
        string? query = null,
        int offset = 0,
        int limit = 10,
        bool includeReviews = false)
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

    /// <summary>Get one movie with its review count, average stars and every review, including each author's name.</summary>
    /// <param name="movieId">The movie's id (a GUID). Use movies_search to find it.</param>
    [McpServerTool(Name = "movies_get", Title = "Get movie", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    public static async partial Task<MovieResult> GetMovie(
        ISender sender,
        Guid movieId,
        CancellationToken cancellationToken)
    {
        var movie = await sender.Send(new Queries.GetMovieById.GetMovieByIdQuery
        {
            Id = movieId
        }, cancellationToken);

        return movie.ToResult(includeReviews: true);
    }
}
