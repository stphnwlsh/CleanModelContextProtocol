namespace CleanModelContextProtocol.Presentation.Tests.Integration.Prompts;

using CleanModelContextProtocol.Presentation.Models;
using Extensions;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;

public class PromptTests : BaseToolTests
{
    [Fact]
    public async Task ListPrompts_ShouldReturn_EveryPrompt()
    {
        // Act
        var prompts = await this.Client.ListPromptsAsync();

        // Assert
        prompts.Select(p => p.Name).ShouldBe(["summarise_movie_reception", "compare_movies", "review_movie"], ignoreOrder: true);

        foreach (var prompt in prompts)
        {
            prompt.Title.ShouldNotBeNullOrWhiteSpace();
            prompt.Description.ShouldNotBeNullOrWhiteSpace();
            prompt.ProtocolPrompt.Arguments.ShouldNotBeEmpty();
            prompt.ProtocolPrompt.Arguments.ShouldAllBe(a => a.Required == true && !string.IsNullOrWhiteSpace(a.Description));
        }
    }

    [Fact]
    public async Task SummariseMovieReception_ShouldEmbed_Movie()
    {
        // Arrange
        var movie = await this.FirstMovieAsync();

        // Act
        var result = await this.Client.GetPromptAsync("summarise_movie_reception",
            new Dictionary<string, object> { ["movieTitle"] = movie.Title.ToUpperInvariant() });

        // Assert
        result.Messages.Count.ShouldBe(2);
        result.Messages.ShouldAllBe(m => m.Role == Role.User);
        result.Messages[0].Content.ShouldBeOfType<TextContentBlock>().Text.ShouldContain(movie.Title);
        ShouldEmbed(result.Messages[1], movie);
    }

    [Fact]
    public async Task SummariseMovieReception_WithUnknownTitle_ShouldAsk_ToSearch()
    {
        // Act
        var result = await this.Client.GetPromptAsync("summarise_movie_reception",
            new Dictionary<string, object> { ["movieTitle"] = "No Such Movie" });

        // Assert
        var message = result.Messages.ShouldHaveSingleItem();
        message.Content.ShouldBeOfType<TextContentBlock>().Text.ShouldContain("Find it with movies_search");
    }

    [Fact]
    public async Task SummariseMovieReception_WithLongTitle_ShouldThrow_InvalidParams()
    {
        // Act
        var exception = await Should.ThrowAsync<McpProtocolException>(() => this.Client.GetPromptAsync("summarise_movie_reception",
            new Dictionary<string, object> { ["movieTitle"] = new string('a', 101) }).AsTask());

        // Assert
        exception.ErrorCode.ShouldBe(McpErrorCode.InvalidParams);
    }

    [Fact]
    public async Task CompareMovies_ShouldEmbed_BothMovies()
    {
        // Arrange
        var movies = (await this.CallAsync<ResultPage<MovieResult>>("movies_search",
            new Dictionary<string, object> { ["limit"] = 2 })).Items;

        // Act
        var result = await this.Client.GetPromptAsync("compare_movies", new Dictionary<string, object>
        {
            ["firstMovieTitle"] = movies[0].Title,
            ["secondMovieTitle"] = movies[1].Title
        });

        // Assert
        result.Messages.Count.ShouldBe(3);
        ShouldEmbed(result.Messages[1], movies[0]);
        ShouldEmbed(result.Messages[2], movies[1]);
    }

    [Fact]
    public async Task ReviewMovie_ShouldReturn_Instructions()
    {
        // Act
        var result = await this.Client.GetPromptAsync("review_movie", new Dictionary<string, object>
        {
            ["movieTitle"] = "Some Movie",
            ["authorName"] = "Some Author"
        });

        // Assert
        var text = result.Messages.ShouldHaveSingleItem().Content.ShouldBeOfType<TextContentBlock>().Text;
        text.ShouldContain("\"Some Movie\"");
        text.ShouldContain("\"Some Author\"");
        text.ShouldContain("reviews_write");
    }

    private async Task<MovieResult> FirstMovieAsync()
    {
        return (await this.CallAsync<ResultPage<MovieResult>>("movies_search", new Dictionary<string, object> { ["limit"] = 50 }))
            .Items.First(m => m.ReviewCount > 0);
    }

    private static void ShouldEmbed(PromptMessage message, MovieResult movie)
    {
        var resource = message.Content.ShouldBeOfType<EmbeddedResourceBlock>().Resource.ShouldBeOfType<TextResourceContents>();
        resource.Uri.ShouldBe($"movies://{movie.Id}");
        resource.MimeType.ShouldBe("application/json");

        var embedded = resource.Text.Deserialize<MovieResult>();
        embedded.Id.ShouldBe(movie.Id);
        embedded.Reviews.Count.ShouldBe(movie.ReviewCount);
    }
}
