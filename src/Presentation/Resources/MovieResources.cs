namespace CleanModelContextProtocol.Presentation.Resources;

using System.ComponentModel;
using CleanModelContextProtocol.Presentation.Models;
using CleanModelContextProtocol.Presentation.Serialization;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Movies.Queries;

[McpServerResourceType]
public static class MovieResources
{
    [McpServerResource(UriTemplate = "movies://{movieId}", Name = "movie", Title = "Movie", MimeType = "application/json")]
    [Description("A movie with its review count, average stars and every review, including each author's name.")]
    public static async Task<string> GetMovie(
        ISender sender,
        [Description("The movie's id (a GUID).")] Guid movieId)
    {
        var movie = await sender.Send(new Queries.GetMovieById.GetMovieByIdQuery
        {
            Id = movieId
        });

        return McpSerializer.Serialize(movie.ToResult(includeReviews: true));
    }
}
