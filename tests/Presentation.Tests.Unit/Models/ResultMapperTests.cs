namespace CleanModelContextProtocol.Presentation.Tests.Unit.Models;

using CleanModelContextProtocol.Application.Authors.Entities;
using CleanModelContextProtocol.Application.Common.Paging;
using CleanModelContextProtocol.Application.Movies.Entities;
using CleanModelContextProtocol.Application.Reviews.Entities;
using CleanModelContextProtocol.Presentation.Models;
using Shouldly;
using Xunit;

public class ResultMapperTests
{
    [Fact]
    public void ToResult_ShouldMap_MovieWithReviews()
    {
        // Arrange
        var author = new ReviewAuthor(Guid.NewGuid(), "First", "Last");
        var movie = new Movie(Guid.NewGuid(), "Title", [new Review(Guid.NewGuid(), 5, ReviewAuthor: author), new Review(Guid.NewGuid(), 2, ReviewAuthor: author)]);

        // Act
        var result = movie.ToResult(includeReviews: true);

        // Assert
        result.Id.ShouldBe(movie.Id);
        result.Title.ShouldBe("Title");
        result.ReviewCount.ShouldBe(2);
        result.AverageStars.ShouldBe(3.5);
        result.Reviews.ShouldNotBeNull().Count.ShouldBe(2);
        result.Reviews.ShouldAllBe(r => r.MovieId == movie.Id && r.MovieTitle == "Title" && r.AuthorName == "First Last");
    }

    [Fact]
    public void ToResult_ShouldLeaveOut_MovieReviewsWhenNotAsked()
    {
        // Arrange
        var movie = new Movie(Guid.NewGuid(), "Title", []);

        // Act
        var result = movie.ToResult(includeReviews: false);

        // Assert
        result.ReviewCount.ShouldBe(0);
        result.AverageStars.ShouldBeNull();
        result.Reviews.ShouldBeNull();
    }

    [Fact]
    public void ToResult_ShouldMap_MovieSummary()
    {
        // Arrange
        var summary = new MovieSummary(Guid.NewGuid(), "Title", 1, 4);
        var reviews = new List<Review> { new(Guid.NewGuid(), 4) };

        // Act
        var withReviews = summary.ToResult(reviews);
        var withoutReviews = summary.ToResult();

        // Assert
        withReviews.ReviewCount.ShouldBe(1);
        withReviews.AverageStars.ShouldBe(4);
        withReviews.Reviews.ShouldNotBeNull().Single().MovieId.ShouldBe(summary.Id);
        withoutReviews.Reviews.ShouldBeNull();
    }

    [Fact]
    public void ToResult_ShouldMap_AuthorWithReviews()
    {
        // Arrange
        var movie = new ReviewedMovie(Guid.NewGuid(), "Title");
        var author = new Author(Guid.NewGuid(), "First", "Last", [new Review(Guid.NewGuid(), 4, movie), new Review(Guid.NewGuid(), 1, movie), new Review(Guid.NewGuid(), 1, movie)]);

        // Act
        var result = author.ToResult(includeReviews: true);

        // Assert
        result.Id.ShouldBe(author.Id);
        result.Name.ShouldBe("First Last");
        result.ReviewCount.ShouldBe(3);
        result.AverageStars.ShouldBe(2);
        result.Reviews.ShouldNotBeNull().ShouldAllBe(r => r.AuthorId == author.Id && r.MovieTitle == "Title");
    }

    [Fact]
    public void ToResult_ShouldLeaveOut_AuthorReviewsWhenNotAsked()
    {
        // Arrange
        var author = new Author(Guid.NewGuid(), "First", "Last", []);

        // Act
        var result = author.ToResult(includeReviews: false);

        // Assert
        result.AverageStars.ShouldBeNull();
        result.Reviews.ShouldBeNull();
    }

    [Fact]
    public void ToResult_ShouldMap_AuthorSummary()
    {
        // Arrange
        var summary = new AuthorSummary(Guid.NewGuid(), "First", "Last", 1, 3);
        var reviews = new List<Review> { new(Guid.NewGuid(), 3) };

        // Act
        var withReviews = summary.ToResult(reviews);
        var withoutReviews = summary.ToResult();

        // Assert
        withReviews.Name.ShouldBe("First Last");
        withReviews.Reviews.ShouldNotBeNull().Single().AuthorName.ShouldBe("First Last");
        withoutReviews.Reviews.ShouldBeNull();
    }

    [Fact]
    public void ToResult_ShouldMap_ReviewWithoutMovieOrAuthor()
    {
        // Arrange
        var review = new Review(Guid.NewGuid(), 3);

        // Act
        var result = review.ToResult();

        // Assert
        result.Stars.ShouldBe(3);
        result.MovieId.ShouldBeNull();
        result.MovieTitle.ShouldBeNull();
        result.AuthorId.ShouldBeNull();
        result.AuthorName.ShouldBeNull();
    }

    [Fact]
    public void ToResult_ShouldPrefer_ReviewsOwnMovieAndAuthor()
    {
        // Arrange
        var movie = new ReviewedMovie(Guid.NewGuid(), "Own Title");
        var author = new ReviewAuthor(Guid.NewGuid(), "Own", "Author");
        var review = new Review(Guid.NewGuid(), 3, movie, author);

        // Act
        var result = review.ToResult(new ReviewedMovie(Guid.NewGuid(), "Other"), new ReviewAuthor(Guid.NewGuid(), "Other", "Author"));

        // Assert
        result.MovieId.ShouldBe(movie.Id);
        result.MovieTitle.ShouldBe("Own Title");
        result.AuthorId.ShouldBe(author.Id);
        result.AuthorName.ShouldBe("Own Author");
    }

    [Fact]
    public void ToResult_ShouldSet_NextOffsetUntilTheLastPage()
    {
        // Arrange
        var middle = new Page<int>([1, 2], 0, 2, 3);
        var last = new Page<int>([3], 2, 2, 3);

        // Act
        var middleResult = middle.ToResult(i => i * 10);
        var lastResult = last.ToResult(i => i * 10);

        // Assert
        middleResult.Items.ShouldBe([10, 20]);
        middleResult.NextOffset.ShouldBe(2);
        lastResult.Items.ShouldBe([30]);
        lastResult.NextOffset.ShouldBeNull();
    }
}
