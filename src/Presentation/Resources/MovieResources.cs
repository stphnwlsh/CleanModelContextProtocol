namespace CleanModelContextProtocol.Presentation.Resources;

using CleanModelContextProtocol.Presentation.Models;
using CleanModelContextProtocol.Presentation.Serialization;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Movies.Queries;

[McpServerResourceType]
public static partial class MovieResources
{
    /// <summary>A movie with its review count, average stars and every review, including each author's name.</summary>
    /// <param name="movieId">The movie's id (a GUID).</param>
    [McpServerResource(UriTemplate = "movies://{movieId}", Name = "movie", Title = "Movie", MimeType = "application/json")]
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
