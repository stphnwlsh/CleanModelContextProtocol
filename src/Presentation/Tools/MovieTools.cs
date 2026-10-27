namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using System.Text.Json;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Movies.Queries;

[McpServerToolType]
public static class MovieTools
{
    [McpServerTool(Name = "GetAllMovies", ReadOnly = true)]
    [Description("Get all movies in the system")]
    public static async Task<string> GetAllMovies(ISender sender)
    {
        var result = await sender.Send(new Queries.GetMovies.GetMoviesQuery());

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "GetMovieById", ReadOnly = true)]
    [Description("Get a specific movie by its unique ID")]
    public static async Task<string> GetMovieById(ISender sender, [Description("The GUID of the movie")] Guid id)
    {
        return JsonSerializer.Serialize(await sender.Send(new Queries.GetMovieById.GetMovieByIdQuery
        {
            Id = id
        }));
    }
}
