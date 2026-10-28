namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using CleanModelContextProtocol.Presentation.Models;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Movies.Queries;

[McpServerToolType]
public static class MovieTools
{
    [McpServerTool(Name = "movies_search", Title = "Search movies", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Find movies whose title contains a search term, ignoring case, in title order. Each result has the movie's id, " +
        "title, review count and average stars. Leave the query empty to browse every movie a page at a time. " +
        "Use this to find a movie's id before calling movies_get, reviews_list or reviews_write.")]
    public static async Task<ResultPage<MovieResult>> SearchMovies(
        ISender sender,
        [Description("Words to look for in the title, such as 'matrix'. Leave empty to list every movie.")] string query = null,
        [Description("The most results to return, from 1 to 50.")] int limit = 10,
        [Description("The nextCursor from a previous call, to get the next page. Leave empty for the first page.")] string cursor = null,
        [Description("Also return every review of each movie. Leave false unless you need the individual reviews.")] bool includeReviews = false)
    {
        var page = await sender.Send(new Queries.SearchMovies.SearchMoviesQuery
        {
            Query = query,
            Limit = limit,
            Cursor = cursor
        });

        return page.ToResult(m => m.ToResult(includeReviews));
    }

    [McpServerTool(Name = "movies_get", Title = "Get movie", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Get one movie with its review count, average stars and every review, including each author's name.")]
    public static async Task<MovieResult> GetMovie(
        ISender sender,
        [Description("The movie's id (a GUID). Use movies_search to find it.")] Guid movieId)
    {
        var movie = await sender.Send(new Queries.GetMovieById.GetMovieByIdQuery
        {
            Id = movieId
        });

        return movie.ToResult(includeReviews: true);
    }
}
