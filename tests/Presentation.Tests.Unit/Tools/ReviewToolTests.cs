namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

using System.Text.Json;
using Application.Authors.Entities;
using Application.Movies.Entities;
using CleanModelContextProtocol.Application.Common.Exceptions;
using CleanModelContextProtocol.Presentation.Tools;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;
using Commands = Application.Reviews.Commands;
using Entities = Application.Reviews.Entities;
using Queries = Application.Reviews.Queries;

public class ReviewToolTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static Entities.Review SampleReview()
    {
        return new(
        Guid.Empty,
        5,
        new ReviewedMovie(Guid.Empty, "Lorem Ipsum"),
        new ReviewAuthor(Guid.Empty, "Lorem", "Ipsum"));
    }

    [Fact]
    public async Task GetAllReviews_ShouldReturn_JsonResult()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetReviews.GetReviewsQuery>())
            .ReturnsForAnyArgs([SampleReview()]);

        // Act
        var response = await ReviewTools.GetAllReviews(sender);

        // Assert
        var result = JsonSerializer.Deserialize<List<Entities.Review>>(response, JsonOptions);

        result.ShouldNotBeNull();
        result[0].Id.ShouldBe(Guid.Empty);
        result[0].Stars.ShouldBe(5);
        result[0].ReviewAuthor.Id.ShouldBe(Guid.Empty);
        result[0].ReviewAuthor.FirstName.ShouldBe("Lorem");
        result[0].ReviewAuthor.LastName.ShouldBe("Ipsum");
        result[0].ReviewedMovie.Id.ShouldBe(Guid.Empty);
        result[0].ReviewedMovie.Title.ShouldBe("Lorem Ipsum");
    }

    [Fact]
    public async Task GetAllReviews_ShouldReturn_Error()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetReviews.GetReviewsQuery>())
            .Throws(new ArgumentException("Expected Exception"));

        // Act
        var response = await ReviewTools.GetAllReviews(sender);

        // Assert
        response.ShouldBe("Error: Expected Exception");
    }

    [Fact]
    public async Task GetReviewById_ShouldReturn_JsonResult()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetReviewById.GetReviewByIdQuery>())
            .ReturnsForAnyArgs(SampleReview());

        // Act
        var response = await ReviewTools.GetReviewById(sender, Guid.Empty);

        // Assert
        var result = JsonSerializer.Deserialize<Entities.Review>(response, JsonOptions);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(Guid.Empty);
        result.Stars.ShouldBe(5);
        result.ReviewAuthor.Id.ShouldBe(Guid.Empty);
        result.ReviewAuthor.FirstName.ShouldBe("Lorem");
        result.ReviewAuthor.LastName.ShouldBe("Ipsum");
        result.ReviewedMovie.Id.ShouldBe(Guid.Empty);
        result.ReviewedMovie.Title.ShouldBe("Lorem Ipsum");
    }

    [Fact]
    public async Task GetReviewById_ShouldReturn_NotFound()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetReviewById.GetReviewByIdQuery>())
            .Throws(new NotFoundException("Expected Exception"));

        // Act
        var response = await ReviewTools.GetReviewById(sender, Guid.Empty);

        // Assert
        response.ShouldBe("Not Found: Expected Exception");
    }

    [Fact]
    public async Task GetReviewById_ShouldReturn_Error()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetReviewById.GetReviewByIdQuery>())
            .Throws(new ArgumentException("Expected Exception"));

        // Act
        var response = await ReviewTools.GetReviewById(sender, Guid.Empty);

        // Assert
        response.ShouldBe("Error: Expected Exception");
    }

    [Fact]
    public async Task CreateReview_ShouldReturn_JsonResult()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Commands.CreateReview.CreateReviewCommand>())
            .ReturnsForAnyArgs(SampleReview());

        // Act
        var response = await ReviewTools.CreateReview(sender, Guid.Empty, Guid.Empty, 5);

        // Assert
        var result = JsonSerializer.Deserialize<Entities.Review>(response, JsonOptions);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(Guid.Empty);
        result.Stars.ShouldBe(5);
        result.ReviewAuthor.Id.ShouldBe(Guid.Empty);
        result.ReviewAuthor.FirstName.ShouldBe("Lorem");
        result.ReviewAuthor.LastName.ShouldBe("Ipsum");
        result.ReviewedMovie.Id.ShouldBe(Guid.Empty);
        result.ReviewedMovie.Title.ShouldBe("Lorem Ipsum");
    }

    [Fact]
    public async Task CreateReview_ShouldReturn_NotFound()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Commands.CreateReview.CreateReviewCommand>())
            .Throws(new NotFoundException("Expected Exception"));

        // Act
        var response = await ReviewTools.CreateReview(sender, Guid.Empty, Guid.Empty, 5);

        // Assert
        response.ShouldBe("Not Found: Expected Exception");
    }

    [Fact]
    public async Task CreateReview_ShouldReturn_Error()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Commands.CreateReview.CreateReviewCommand>())
            .Throws(new ArgumentException("Expected Exception"));

        // Act
        var response = await ReviewTools.CreateReview(sender, Guid.Empty, Guid.Empty, 5);

        // Assert
        response.ShouldBe("Error: Expected Exception");
    }

    [Fact]
    public async Task UpdateReview_ShouldReturn_Success()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Commands.UpdateReview.UpdateReviewCommand>())
            .ReturnsForAnyArgs(true);

        // Act
        var response = await ReviewTools.UpdateReview(sender, Guid.Empty, Guid.Empty, Guid.Empty, 5);

        // Assert
        response.ShouldBe("Review updated successfully");
    }

    [Fact]
    public async Task UpdateReview_ShouldReturn_NotFound()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Commands.UpdateReview.UpdateReviewCommand>())
            .Throws(new NotFoundException("Expected Exception"));

        // Act
        var response = await ReviewTools.UpdateReview(sender, Guid.Empty, Guid.Empty, Guid.Empty, 5);

        // Assert
        response.ShouldBe("Not Found: Expected Exception");
    }

    [Fact]
    public async Task UpdateReview_ShouldReturn_Error()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Commands.UpdateReview.UpdateReviewCommand>())
            .Throws(new ArgumentException("Expected Exception"));

        // Act
        var response = await ReviewTools.UpdateReview(sender, Guid.Empty, Guid.Empty, Guid.Empty, 5);

        // Assert
        response.ShouldBe("Error: Expected Exception");
    }

    [Fact]
    public async Task DeleteReview_ShouldReturn_Success()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Commands.DeleteReview.DeleteReviewCommand>())
            .ReturnsForAnyArgs(true);

        // Act
        var response = await ReviewTools.DeleteReview(sender, Guid.Empty);

        // Assert
        response.ShouldBe("Review deleted successfully");
    }

    [Fact]
    public async Task DeleteReview_ShouldReturn_NotFound()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Commands.DeleteReview.DeleteReviewCommand>())
            .Throws(new NotFoundException("Expected Exception"));

        // Act
        var response = await ReviewTools.DeleteReview(sender, Guid.Empty);

        // Assert
        response.ShouldBe("Not Found: Expected Exception");
    }

    [Fact]
    public async Task DeleteReview_ShouldReturn_Error()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Commands.DeleteReview.DeleteReviewCommand>())
            .Throws(new ArgumentException("Expected Exception"));

        // Act
        var response = await ReviewTools.DeleteReview(sender, Guid.Empty);

        // Assert
        response.ShouldBe("Error: Expected Exception");
    }
}
