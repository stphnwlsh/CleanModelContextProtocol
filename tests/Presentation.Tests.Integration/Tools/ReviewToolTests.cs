namespace CleanModelContextProtocol.Presentation.Tests.Integration.Tools;

using CleanModelContextProtocol.Presentation.Models;
using Shouldly;
using Xunit;

public class ReviewToolTests : BaseToolTests
{
    private async Task<(MovieResult Movie, AuthorResult Author)> FirstMovieAndAuthor()
    {
        var movie = (await this.CallAsync<ResultPage<MovieResult>>("movies_search")).Items[0];
        var author = (await this.CallAsync<ResultPage<AuthorResult>>("authors_search")).Items[0];

        return (movie, author);
    }

    [Fact]
    public async Task ListReviews_ShouldReturn_FirstPage()
    {
        // Act
        var page = await this.CallAsync<ResultPage<ReviewResult>>("reviews_list");

        // Assert
        page.TotalCount.ShouldBe(150);
        page.Items.Count.ShouldBe(10);
        page.NextCursor.ShouldNotBeNull();

        foreach (var review in page.Items)
        {
            review.Stars.ShouldBeInRange(1, 5);
            review.MovieTitle.ShouldNotBeNullOrWhiteSpace();
            review.AuthorName.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task ListReviews_ShouldFilter_ByMovieAndAuthor()
    {
        // Arrange
        var movie = (await this.CallAsync<ResultPage<MovieResult>>("movies_search",
            new Dictionary<string, object> { ["limit"] = 50 })).Items.First(m => m.ReviewCount > 0);
        var review = (await this.CallAsync<MovieResult>("movies_get", new Dictionary<string, object> { ["movieId"] = movie.Id })).Reviews[0];

        // Act
        var byMovie = await this.CallAsync<ResultPage<ReviewResult>>("reviews_list",
            new Dictionary<string, object> { ["movieId"] = movie.Id, ["limit"] = 50 });
        var byBoth = await this.CallAsync<ResultPage<ReviewResult>>("reviews_list",
            new Dictionary<string, object> { ["movieId"] = movie.Id, ["authorId"] = review.AuthorId });

        // Assert
        byMovie.TotalCount.ShouldBe(movie.ReviewCount);
        byMovie.Items.ShouldAllBe(r => r.MovieId == movie.Id);
        byBoth.Items.ShouldContain(r => r.Id == review.Id);
        byBoth.Items.ShouldAllBe(r => r.MovieId == movie.Id && r.AuthorId == review.AuthorId);
    }

    [Fact]
    public async Task ListReviews_ShouldReturn_OneReview()
    {
        // Arrange
        var first = (await this.CallAsync<ResultPage<ReviewResult>>("reviews_list")).Items[0];

        // Act
        var page = await this.CallAsync<ResultPage<ReviewResult>>("reviews_list", new Dictionary<string, object> { ["reviewId"] = first.Id });

        // Assert
        page.TotalCount.ShouldBe(1);
        page.Items.ShouldHaveSingleItem().ShouldBe(first);
    }

    [Fact]
    public async Task ListReviews_WithUnknownReviewId_ShouldReturn_NotFoundError()
    {
        // Act
        var text = await this.CallForErrorAsync("reviews_list", new Dictionary<string, object> { ["reviewId"] = Guid.NewGuid() });

        // Assert
        text.ShouldBe("Not Found: The Review with the supplied id was not found. Check the ids: movies_search, authors_search and reviews_list return valid ones.");
    }

    [Fact]
    public async Task ListReviews_WithEmptyMovieId_ShouldReturn_ValidationError()
    {
        // Act
        var text = await this.CallForErrorAsync("reviews_list", new Dictionary<string, object> { ["movieId"] = Guid.Empty });

        // Assert
        text.ShouldStartWith("Validation Failed: MovieId: A Movie Id can't be empty.");
    }

    [Fact]
    public async Task WriteReview_ShouldCreate_Review()
    {
        // Arrange
        var (movie, author) = await this.FirstMovieAndAuthor();

        // Act
        var review = await this.CallAsync<ReviewResult>("reviews_write",
            new Dictionary<string, object> { ["movieId"] = movie.Id, ["authorId"] = author.Id, ["stars"] = 5 });

        // Assert
        review.Id.ShouldNotBe(Guid.Empty);
        review.Stars.ShouldBe(5);
        review.MovieId.ShouldBe(movie.Id);
        review.MovieTitle.ShouldBe(movie.Title);
        review.AuthorId.ShouldBe(author.Id);
        review.AuthorName.ShouldBe(author.Name);

        var after = await this.CallAsync<MovieResult>("movies_get", new Dictionary<string, object> { ["movieId"] = movie.Id });
        after.ReviewCount.ShouldBe(movie.ReviewCount + 1);
    }

    [Fact]
    public async Task WriteReview_ShouldUpdate_Review()
    {
        // Arrange
        var (movie, author) = await this.FirstMovieAndAuthor();
        var existing = (await this.CallAsync<ResultPage<ReviewResult>>("reviews_list")).Items[0];

        // Act
        var review = await this.CallAsync<ReviewResult>("reviews_write",
            new Dictionary<string, object> { ["reviewId"] = existing.Id, ["movieId"] = movie.Id, ["authorId"] = author.Id, ["stars"] = 1 });

        // Assert
        review.Id.ShouldBe(existing.Id);
        review.Stars.ShouldBe(1);
        review.MovieId.ShouldBe(movie.Id);
        review.AuthorId.ShouldBe(author.Id);
    }

    [Fact]
    public async Task WriteReview_WithInvalidStars_ShouldReturn_ValidationError()
    {
        // Arrange
        var (movie, author) = await this.FirstMovieAndAuthor();

        // Act
        var text = await this.CallForErrorAsync("reviews_write",
            new Dictionary<string, object> { ["movieId"] = movie.Id, ["authorId"] = author.Id, ["stars"] = 9 });

        // Assert
        text.ShouldBe("Validation Failed: Stars: A star rating must be between 1 and 5. Correct the arguments and call the tool again.");
    }

    [Fact]
    public async Task WriteReview_WithUnknownReviewId_ShouldReturn_NotFoundError()
    {
        // Arrange
        var (movie, author) = await this.FirstMovieAndAuthor();

        // Act
        var text = await this.CallForErrorAsync("reviews_write",
            new Dictionary<string, object> { ["reviewId"] = Guid.NewGuid(), ["movieId"] = movie.Id, ["authorId"] = author.Id, ["stars"] = 3 });

        // Assert
        text.ShouldStartWith("Not Found:");
    }

    [Fact]
    public async Task DeleteReview_ShouldDelete_Review()
    {
        // Arrange
        var existing = (await this.CallAsync<ResultPage<ReviewResult>>("reviews_list")).Items[0];

        // Act
        var result = await this.CallAsync<DeletedReview>("reviews_delete", new Dictionary<string, object> { ["reviewId"] = existing.Id });

        // Assert
        result.ReviewId.ShouldBe(existing.Id);
        result.Deleted.ShouldBeTrue();

        var text = await this.CallForErrorAsync("reviews_list", new Dictionary<string, object> { ["reviewId"] = existing.Id });
        text.ShouldStartWith("Not Found:");
    }

    [Fact]
    public async Task DeleteReview_WithUnknownId_ShouldReturn_NotFoundError()
    {
        // Act
        var text = await this.CallForErrorAsync("reviews_delete", new Dictionary<string, object> { ["reviewId"] = Guid.NewGuid() });

        // Assert
        text.ShouldStartWith("Not Found: The Review with the supplied id was not found.");
    }
}
