namespace CleanModelContextProtocol.Presentation.Tests.Integration.Completions;

using CleanModelContextProtocol.Presentation.Models;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;

public class CompletionTests : BaseToolTests
{
    [Fact]
    public async Task CompleteMovieTitle_ShouldReturn_MatchingTitles()
    {
        // Arrange
        var movie = (await this.CallAsync<ResultPage<MovieResult>>("movies_search")).Items[0];
        var partial = movie.Title[..Math.Min(3, movie.Title.Length)];

        // Act
        var result = await this.Client.CompleteAsync(new PromptReference { Name = "summarise_movie_reception" }, "movieTitle", partial);

        // Assert
        result.Completion.Values.ShouldContain(movie.Title);
        result.Completion.Values.ShouldAllBe(v => v.Contains(partial, StringComparison.OrdinalIgnoreCase));
        result.Completion.Total.ShouldBe(result.Completion.Values.Count);
        result.Completion.HasMore.ShouldBe(false);
    }

    [Fact]
    public async Task CompleteMovieTitle_WithEmptyValue_ShouldReturn_FirstPage()
    {
        // Act
        var result = await this.Client.CompleteAsync(new PromptReference { Name = "compare_movies" }, "secondMovieTitle", string.Empty);

        // Assert
        result.Completion.Values.Count.ShouldBe(50);
        result.Completion.Total.ShouldBe(50);
        result.Completion.HasMore.ShouldBe(false);
    }

    [Fact]
    public async Task CompleteAuthorName_ShouldReturn_MatchingNames()
    {
        // Arrange
        var author = (await this.CallAsync<ResultPage<AuthorResult>>("authors_search")).Items[0];

        // Act
        var result = await this.Client.CompleteAsync(new PromptReference { Name = "review_movie" }, "authorName", author.Name);

        // Assert
        result.Completion.Values.ShouldContain(author.Name);
    }

    [Theory]
    [InlineData("review_movie", "unknown", "a")]
    [InlineData("review_movie", "authorName", "this value is far too long to be a search term because it runs past the one hundred character limit set by the search")]
    public async Task Complete_WithUnsupportedArgument_ShouldReturn_Nothing(string prompt, string argument, string value)
    {
        // Act
        var result = await this.Client.CompleteAsync(new PromptReference { Name = prompt }, argument, value);

        // Assert
        result.Completion.Values.ShouldBeEmpty();
        result.Completion.HasMore.ShouldBe(false);
    }

    [Fact]
    public async Task Complete_ResourceTemplate_ShouldReturn_Nothing()
    {
        // Act
        var result = await this.Client.CompleteAsync(new ResourceTemplateReference { Uri = "movies://{movieId}" }, "movieId", "a");

        // Assert
        result.Completion.Values.ShouldBeEmpty();
    }
}
