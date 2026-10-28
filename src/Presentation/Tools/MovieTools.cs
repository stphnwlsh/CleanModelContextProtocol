namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using Mediator;
using ModelContextProtocol.Server;
using Entities = Application.Movies.Entities;
using Queries = Application.Movies.Queries;

[McpServerToolType]
public static class MovieTools
{
    [McpServerTool(Name = "movies_list", Title = "List movies", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("List every movie with its id, title and reviews. " +
        "Use this to find a movie's id before calling movies_get, reviews_create or reviews_update.")]
    public static async Task<List<Entities.Movie>> ListMovies(ISender sender)
    {
        return await sender.Send(new Queries.GetMovies.GetMoviesQuery());
    }

    [McpServerTool(Name = "movies_get", Title = "Get movie", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Get one movie by id, including every review of it with the star rating and the author who wrote it.")]
    public static async Task<Entities.Movie> GetMovie(
        ISender sender,
        [Description("The movie's id (a GUID). Use movies_list to find it.")] Guid movieId)
    {
        return await sender.Send(new Queries.GetMovieById.GetMovieByIdQuery
        {
            Id = movieId
        });
    }
}
