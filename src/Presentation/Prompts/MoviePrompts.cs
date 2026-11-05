namespace CleanModelContextProtocol.Presentation.Prompts;

using Mediator;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Presentation.Models;
using Presentation.Serialization;
using Queries = Application.Movies.Queries;

[McpServerPromptType]
public static partial class MoviePrompts
{
    /// <summary>Summarise how reviewers have received a movie: its average stars, how the ratings spread, and who liked it most and least.</summary>
    /// <param name="movieTitle">The movie's title, or part of it.</param>
    [McpServerPrompt(Name = "summarise_movie_reception", Title = "Summarise a movie's reception")]
    public static async partial Task<GetPromptResult> SummariseMovieReception(
        ISender sender,
        string movieTitle,
        CancellationToken cancellationToken)
    {
        var movie = await FindMovie(sender, movieTitle, cancellationToken);

        var messages = new List<PromptMessage>
        {
            Text(movie is null
                ? $"Summarise how reviewers have received the movie \"{movieTitle}\". Find it with movies_search, get its reviews " +
                    "with movies_get, then give its average stars, how the ratings spread from 1 to 5, and which authors rated it highest and lowest."
                : $"Summarise how reviewers have received \"{movie.Title}\", using the movie below. Give its average stars, " +
                    "how the ratings spread from 1 to 5, and which authors rated it highest and lowest.")
        };

        if (movie is not null)
        {
            messages.Add(Embed(movie));
        }

        return new GetPromptResult
        {
            Description = $"Summarise the reception of {movie?.Title ?? movieTitle}",
            Messages = messages
        };
    }

    /// <summary>Compare how reviewers rated two movies, and say which one they preferred and by how much.</summary>
    /// <param name="firstMovieTitle">The first movie's title, or part of it.</param>
    /// <param name="secondMovieTitle">The second movie's title, or part of it.</param>
    [McpServerPrompt(Name = "compare_movies", Title = "Compare two movies")]
    public static async partial Task<GetPromptResult> CompareMovies(
        ISender sender,
        string firstMovieTitle,
        string secondMovieTitle,
        CancellationToken cancellationToken)
    {
        var first = await FindMovie(sender, firstMovieTitle, cancellationToken);
        var second = await FindMovie(sender, secondMovieTitle, cancellationToken);

        var messages = new List<PromptMessage>
        {
            Text($"Compare how reviewers rated \"{first?.Title ?? firstMovieTitle}\" and \"{second?.Title ?? secondMovieTitle}\". " +
                "Give each movie's average stars and number of reviews, note any authors who reviewed both, and say which movie " +
                "reviewers preferred. Find any movie that isn't attached below with movies_search, then get it with movies_get.")
        };

        foreach (var movie in new[] { first, second }.OfType<MovieResult>())
        {
            messages.Add(Embed(movie));
        }

        return new GetPromptResult
        {
            Description = $"Compare {first?.Title ?? firstMovieTitle} with {second?.Title ?? secondMovieTitle}",
            Messages = messages
        };
    }

    /// <summary>Write a review of a movie as one of the authors, checking the movie and author before saving it.</summary>
    /// <param name="movieTitle">The movie's title, or part of it.</param>
    /// <param name="authorName">The name of the author writing the review.</param>
    [McpServerPrompt(Name = "review_movie", Title = "Review a movie")]
    public static partial GetPromptResult ReviewMovie(
        string movieTitle,
        string authorName)
    {
        return new GetPromptResult
        {
            Description = $"Review {movieTitle} as {authorName}",
            Messages =
            [
                Text($"I want to review the movie \"{movieTitle}\" as the author \"{authorName}\". Find the movie with movies_search " +
                    "and the author with authors_search. If either search doesn't return exactly one clear match, show me the closest " +
                    "results and ask which one I meant. Ask me for a star rating from 1 to 5, then save the review with reviews_write " +
                    "and show me the saved review.")
            ]
        };
    }

    private static async Task<MovieResult?> FindMovie(ISender sender, string? title, CancellationToken cancellationToken)
    {
        var page = await sender.Send(new Queries.GetMovies.GetMoviesQuery
        {
            Query = title,
            Limit = 5
        }, cancellationToken);

        var movie = page.Items.FirstOrDefault(m => string.Equals(m.Title, title?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? (page.TotalCount == 1 ? page.Items[0] : null);

        if (movie is null)
        {
            return null;
        }

        // The search only has the review count and average, so get the movie again for its reviews.
        var withReviews = await sender.Send(new Queries.GetMovieById.GetMovieByIdQuery { Id = movie.Id }, cancellationToken);

        return withReviews.ToResult(includeReviews: true);
    }

    private static PromptMessage Text(string text)
    {
        return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock { Text = text }
        };
    }

    private static PromptMessage Embed(MovieResult movie)
    {
        return new PromptMessage
        {
            Role = Role.User,
            Content = new EmbeddedResourceBlock
            {
                Resource = new TextResourceContents
                {
                    Uri = $"movies://{movie.Id}",
                    MimeType = "application/json",
                    Text = McpSerializer.Serialize(movie)
                }
            }
        };
    }
}
