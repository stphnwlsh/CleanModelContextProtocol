namespace CleanModelContextProtocol.Presentation.Resources;

using Application.Common.Paging;
using Application.Movies.Entities;
using Mediator;
using ModelContextProtocol.Server;
using Presentation.Models;
using Presentation.Serialization;
using Presentation.Tools;
using Queries = Application.Movies.Queries;

[McpServerResourceType]
public static partial class MovieResources
{
    private const int TopRatedCount = 10;

    /// <summary>The ten best rated movies, highest average stars first, with each one's review count.</summary>
    [McpServerResource(UriTemplate = "movies://top-rated", Name = "top_rated_movies", Title = "Top rated movies", MimeType = "application/json", IconSource = ItemIcons.Movie)]
    public static async partial Task<string> GetTopRatedMovies(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var movies = new List<MovieSummary>();
        Page<MovieSummary> page;

        // The query sorts by title, so ranking needs every movie.
        do
        {
            page = await sender.Send(new Queries.GetMovies.GetMoviesQuery
            {
                Offset = movies.Count,
                Limit = Paging.MaxLimit
            }, cancellationToken);

            movies.AddRange(page.Items);
        }
        while (movies.Count < page.TotalCount);

        var topRated = movies
            .Where(m => m.ReviewCount > 0)
            .OrderByDescending(m => m.AverageStars)
            .ThenByDescending(m => m.ReviewCount)
            .ThenBy(m => m.Title, StringComparer.Ordinal)
            .Take(TopRatedCount)
            .Select(m => m.ToResult())
            .ToList();

        return McpSerializer.Serialize(new ResultPage<MovieResult>(topRated, topRated.Count, null));
    }

    /// <summary>A movie with its review count, average stars and every review, including each author's name.</summary>
    /// <param name="movieId">The movie's id (a GUID).</param>
    [McpServerResource(UriTemplate = "movies://{movieId}", Name = "movie", Title = "Movie", MimeType = "application/json", IconSource = ItemIcons.Movie)]
    public static async partial Task<string> GetMovie(
        ISender sender,
        Guid movieId,
        CancellationToken cancellationToken)
    {
        var movie = await sender.Send(new Queries.GetMovieById.GetMovieByIdQuery
        {
            Id = movieId
        }, cancellationToken);

        return McpSerializer.Serialize(movie.ToResult(includeReviews: true));
    }
}
