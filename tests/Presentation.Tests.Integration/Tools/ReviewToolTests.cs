namespace CleanModelContextProtocol.Presentation.Tests.Integration.Tools;

using Extensions;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;
using AuthorEntities = Application.Authors.Entities;
using Entities = Application.Reviews.Entities;
using MovieEntities = Application.Movies.Entities;

public class ReviewToolTests : BaseToolTests
{
    [Fact]
    public async Task GetAllReviews_ShouldReturn_Reviews()
    {
        // Act
        var result = await this.Client.CallToolAsync("GetAllReviews");
        var text = result.Content.OfType<TextContentBlock>().First().Text;
        var reviews = text.Deserialize<List<Entities.Review>>();

        // Assert
        _ = reviews.ShouldNotBeNull();

        reviews.ShouldNotBeEmpty();
        reviews.Count.ShouldBe(150);

        foreach (var review in reviews)
        {
            _ = review.ShouldNotBeNull();
            _ = review.Id.ShouldBeOfType<Guid>();
            _ = review.Stars.ShouldBeOfType<int>();
            review.Stars.ShouldBeInRange(1, 5);

            _ = review.ReviewAuthor.ShouldNotBeNull();
            _ = review.ReviewAuthor.Id.ShouldBeOfType<Guid>();
            _ = review.ReviewAuthor.FirstName.ShouldBeOfType<string>();
            review.ReviewAuthor.FirstName.ShouldNotBeNullOrWhiteSpace();
            _ = review.ReviewAuthor.LastName.ShouldBeOfType<string>();
            review.ReviewAuthor.LastName.ShouldNotBeNullOrWhiteSpace();

            _ = review.ReviewedMovie.ShouldNotBeNull();
            _ = review.ReviewedMovie.Id.ShouldBeOfType<Guid>();
            _ = review.ReviewedMovie.Title.ShouldBeOfType<string>();
            review.ReviewedMovie.Title.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetReviewById_ShouldReturn_Review()
    {
        // Arrange
        var allResult = await this.Client.CallToolAsync("GetAllReviews");
        var allText = allResult.Content.OfType<TextContentBlock>().First().Text;
        var firstReview = allText.Deserialize<List<Entities.Review>>()[0];

        // Act
        var result = await this.Client.CallToolAsync("GetReviewById",
            new Dictionary<string, object> { ["id"] = firstReview.Id });
        var text = result.Content.OfType<TextContentBlock>().First().Text;
        var review = text.Deserialize<Entities.Review>();

        // Assert
        _ = review.ShouldNotBeNull();

        _ = review.Id.ShouldBeOfType<Guid>();
        review.Id.ShouldBe(firstReview.Id);
        _ = review.Stars.ShouldBeOfType<int>();
        review.Stars.ShouldBeInRange(1, 5);

        _ = review.ReviewAuthor.ShouldNotBeNull();
        _ = review.ReviewAuthor.Id.ShouldBeOfType<Guid>();
        _ = review.ReviewAuthor.FirstName.ShouldBeOfType<string>();
        review.ReviewAuthor.FirstName.ShouldNotBeNullOrWhiteSpace();
        _ = review.ReviewAuthor.LastName.ShouldBeOfType<string>();
        review.ReviewAuthor.LastName.ShouldNotBeNullOrWhiteSpace();

        _ = review.ReviewedMovie.ShouldNotBeNull();
        _ = review.ReviewedMovie.Id.ShouldBeOfType<Guid>();
        _ = review.ReviewedMovie.Title.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task CreateReview_ShouldReturn_Review()
    {
        // Arrange
        var authorResult = await this.Client.CallToolAsync("GetAllAuthors");
        var authorText = authorResult.Content.OfType<TextContentBlock>().First().Text;
        var author = authorText.Deserialize<List<AuthorEntities.Author>>()[0];

        var movieResult = await this.Client.CallToolAsync("GetAllMovies");
        var movieText = movieResult.Content.OfType<TextContentBlock>().First().Text;
        var movie = movieText.Deserialize<List<MovieEntities.Movie>>()[0];

        // Act
        var result = await this.Client.CallToolAsync("CreateReview",
            new Dictionary<string, object>
            {
                ["authorId"] = author.Id,
                ["movieId"] = movie.Id,
                ["stars"] = 5
            });
        var text = result.Content.OfType<TextContentBlock>().First().Text;
        var review = text.Deserialize<Entities.Review>();

        // Assert
        _ = review.ShouldNotBeNull();

        _ = review.Id.ShouldBeOfType<Guid>();
        _ = review.Stars.ShouldBeOfType<int>();
        review.Stars.ShouldBe(5);

        _ = review.ReviewAuthor.ShouldNotBeNull();
        review.ReviewAuthor.Id.ShouldBe(author.Id);
        review.ReviewAuthor.FirstName.ShouldBe(author.FirstName);
        review.ReviewAuthor.LastName.ShouldBe(author.LastName);

        _ = review.ReviewedMovie.ShouldNotBeNull();
        review.ReviewedMovie.Id.ShouldBe(movie.Id);
        review.ReviewedMovie.Title.ShouldBe(movie.Title);
    }

    [Fact]
    public async Task UpdateReview_ShouldReturn_Success()
    {
        // Arrange
        var authorResult = await this.Client.CallToolAsync("GetAllAuthors");
        var authorText = authorResult.Content.OfType<TextContentBlock>().First().Text;
        var author = authorText.Deserialize<List<AuthorEntities.Author>>()[0];

        var movieResult = await this.Client.CallToolAsync("GetAllMovies");
        var movieText = movieResult.Content.OfType<TextContentBlock>().First().Text;
        var movie = movieText.Deserialize<List<MovieEntities.Movie>>()[0];

        var reviewsResult = await this.Client.CallToolAsync("GetAllReviews");
        var reviewsText = reviewsResult.Content.OfType<TextContentBlock>().First().Text;
        var existingReview = reviewsText.Deserialize<List<Entities.Review>>()[0];

        // Act
        var result = await this.Client.CallToolAsync("UpdateReview",
            new Dictionary<string, object>
            {
                ["id"] = existingReview.Id,
                ["authorId"] = author.Id,
                ["movieId"] = movie.Id,
                ["stars"] = 5
            });
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Assert
        text.ShouldBe("Review updated successfully");

        var validateResult = await this.Client.CallToolAsync("GetReviewById",
            new Dictionary<string, object> { ["id"] = existingReview.Id });
        var validateText = validateResult.Content.OfType<TextContentBlock>().First().Text;
        var updatedReview = validateText.Deserialize<Entities.Review>();

        _ = updatedReview.ShouldNotBeNull();

        updatedReview.Id.ShouldBe(existingReview.Id);
        updatedReview.Stars.ShouldBe(5);

        _ = updatedReview.ReviewAuthor.ShouldNotBeNull();
        updatedReview.ReviewAuthor.Id.ShouldBe(author.Id);
        updatedReview.ReviewAuthor.FirstName.ShouldBe(author.FirstName);
        updatedReview.ReviewAuthor.LastName.ShouldBe(author.LastName);

        _ = updatedReview.ReviewedMovie.ShouldNotBeNull();
        updatedReview.ReviewedMovie.Id.ShouldBe(movie.Id);
        updatedReview.ReviewedMovie.Title.ShouldBe(movie.Title);
    }

    [Fact]
    public async Task DeleteReview_ShouldReturn_Success()
    {
        // Arrange
        var reviewsResult = await this.Client.CallToolAsync("GetAllReviews");
        var reviewsText = reviewsResult.Content.OfType<TextContentBlock>().First().Text;
        var existingReview = reviewsText.Deserialize<List<Entities.Review>>()[0];

        // Act
        var result = await this.Client.CallToolAsync("DeleteReview",
            new Dictionary<string, object> { ["id"] = existingReview.Id });
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Assert
        text.ShouldBe("Review deleted successfully");

        var validateResult = await this.Client.CallToolAsync("GetReviewById",
            new Dictionary<string, object> { ["id"] = existingReview.Id });
        var validateText = validateResult.Content.OfType<TextContentBlock>().First().Text;

        validateText.ShouldStartWith("Not Found:");
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("1")]
    [InlineData("fake")]
    public async Task GetReviewById_WithInvalidId_ShouldReturn_Error(string input)
    {
        // Valid-format GUIDs (including empty) pass as Guid; unparseable strings pass as string
        object idArg = Guid.TryParse(input, out var guid) ? guid : input;

        try
        {
            // Act
            var result = await this.Client.CallToolAsync("GetReviewById",
                new Dictionary<string, object> { ["id"] = idArg });

            // Assert - empty GUID returns "Not Found:" text; binding errors may set IsError
            var isError = result.IsError == true;
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text ?? string.Empty;
            var isTextError = textContent.StartsWith("Not Found:", StringComparison.Ordinal)
                || textContent.StartsWith("Error:", StringComparison.Ordinal);

            (isError || isTextError).ShouldBeTrue();
        }
        catch (McpException)
        {
            // Protocol-level error for unparseable inputs - expected
        }
    }
}
