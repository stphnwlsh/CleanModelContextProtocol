namespace CleanModelContextProtocol.Presentation.Tests.Integration.Completions;

using ModelContextProtocol.Protocol;
using Presentation.Models;
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
    public async Task CompleteAuthorName_OnAuthorPrompt_ShouldReturn_MatchingNames()
    {
        // Arrange
        var author = (await this.CallAsync<ResultPage<AuthorResult>>("authors_search")).Items[0];

        // Act
        var result = await this.Client.CompleteAsync(new PromptReference { Name = "summarise_author_reviews" }, "authorName", author.Name[..3]);

        // Assert
        result.Completion.Values.ShouldContain(author.Name);
    }

    [Fact]
    public async Task CompleteMovieId_ShouldMatch_Title()
    {
        // Arrange
        var movie = (await this.CallAsync<ResultPage<MovieResult>>("movies_search")).Items[0];

        // Act
        var result = await this.Client.CompleteAsync(new ResourceTemplateReference { Uri = "movies://{movieId}" }, "movieId", movie.Title);

        // Assert
        result.Completion.Values.ShouldContain(movie.Id.ToString());
    }

    [Fact]
    public async Task CompleteMovieId_ShouldMatch_IdPrefix()
    {
        // Arrange
        var movie = (await this.CallAsync<ResultPage<MovieResult>>("movies_search")).Items[0];

        // Act
        var result = await this.Client.CompleteAsync(new ResourceTemplateReference { Uri = "movies://{movieId}" }, "movieId", movie.Id.ToString()[..8]);

        // Assert
        result.Completion.Values.ShouldBe([movie.Id.ToString()]);
    }

    [Fact]
    public async Task CompleteAuthorId_ShouldMatch_Name()
    {
        // Arrange
        var author = (await this.CallAsync<ResultPage<AuthorResult>>("authors_search")).Items[0];

        // Act
        var result = await this.Client.CompleteAsync(new ResourceTemplateReference { Uri = "authors://{authorId}" }, "authorId", author.Name);

        // Assert
        result.Completion.Values.ShouldContain(author.Id.ToString());
    }

    [Fact]
    public async Task CompleteAuthorId_ShouldMatch_IdPrefix()
    {
        // Arrange
        var author = (await this.CallAsync<ResultPage<AuthorResult>>("authors_search")).Items[0];

        // Act
        var result = await this.Client.CompleteAsync(new ResourceTemplateReference { Uri = "authors://{authorId}" }, "authorId", author.Id.ToString()[..8]);

        // Assert
        result.Completion.Values.ShouldBe([author.Id.ToString()]);
    }

    [Fact]
    public async Task CompleteReviewId_ShouldMatch_IdPrefix()
    {
        // Arrange
        var review = (await this.CallAsync<ResultPage<ReviewResult>>("reviews_list")).Items[0];

        // Act
        var result = await this.Client.CompleteAsync(new ResourceTemplateReference { Uri = "reviews://{reviewId}" }, "reviewId", review.Id.ToString()[..8]);

        // Assert
        result.Completion.Values.ShouldContain(review.Id.ToString());
    }

    [Fact]
    public async Task CompleteReviewId_ShouldMatch_MovieTitle()
    {
        // Arrange
        var review = (await this.CallAsync<ResultPage<ReviewResult>>("reviews_list")).Items[0];

        // Act
        var result = await this.Client.CompleteAsync(new ResourceTemplateReference { Uri = "reviews://{reviewId}" }, "reviewId", review.MovieTitle!);

        // Assert
        result.Completion.Values.ShouldContain(review.Id.ToString());
    }

    [Theory]
    [InlineData("movies://{movieId}", "movieId")]
    [InlineData("authors://{authorId}", "authorId")]
    [InlineData("reviews://{reviewId}", "reviewId")]
    public async Task CompleteResourceId_WithEmptyValue_ShouldReturn_Ids(string uri, string argument)
    {
        // Act
        var result = await this.Client.CompleteAsync(new ResourceTemplateReference { Uri = uri }, argument, string.Empty);

        // Assert
        result.Completion.Values.ShouldNotBeEmpty();

        foreach (var value in result.Completion.Values)
        {
            Guid.TryParse(value, out _).ShouldBeTrue();
        }
    }

    [Theory]
    [InlineData("movies://{movieId}", "movieId")]
    [InlineData("authors://{authorId}", "authorId")]
    [InlineData("reviews://{reviewId}", "reviewId")]
    public async Task CompleteResourceId_WithNoMatch_ShouldReturn_Nothing(string uri, string argument)
    {
        // Act
        var result = await this.Client.CompleteAsync(new ResourceTemplateReference { Uri = uri }, argument, "zzz-no-match");

        // Assert
        result.Completion.Values.ShouldBeEmpty();
        result.Completion.Total.ShouldBe(0);
    }

    [Fact]
    public async Task CompleteUnknownResourceArgument_ShouldReturn_Nothing()
    {
        // Act
        var result = await this.Client.CompleteAsync(new ResourceTemplateReference { Uri = "movies://{movieId}" }, "unknown", "a");

        // Assert
        result.Completion.Values.ShouldBeEmpty();
    }
}
