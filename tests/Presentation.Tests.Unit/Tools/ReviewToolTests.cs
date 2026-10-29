namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

using Application.Authors.Entities;
using Application.Common.Paging;
using Application.Movies.Entities;
using CleanModelContextProtocol.Presentation.Tools;
using Mediator;
using NSubstitute;
using Shouldly;
using Xunit;
using Commands = Application.Reviews.Commands;
using Entities = Application.Reviews.Entities;
using Queries = Application.Reviews.Queries;

public class ReviewToolTests
{
    private static readonly Guid ReviewId = Guid.NewGuid();
    private static readonly Guid MovieId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    private static Entities.Review SampleReview(int stars = 5)
    {
        return new(ReviewId, stars, new ReviewedMovie(MovieId, "Lorem Ipsum"), new ReviewAuthor(AuthorId, "Dolor", "Sit"));
    }

    [Fact]
    public async Task ListReviews_ShouldReturn_Page()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Is<Queries.GetReviews.GetReviewsQuery>(q => q.MovieId == MovieId && q.AuthorId == AuthorId && q.Offset == 2 && q.Limit == 1))
            .Returns(new Page<Entities.Review>([SampleReview()], 2, 1, 3));

        // Act
        var response = await ReviewTools.ListReviews(sender, CancellationToken.None, MovieId, AuthorId, null, 2, 1);

        // Assert
        response.TotalCount.ShouldBe(3);
        response.NextOffset.ShouldBeNull();
        response.Items[0].Id.ShouldBe(ReviewId);
        response.Items[0].Stars.ShouldBe(5);
        response.Items[0].MovieId.ShouldBe(MovieId);
        response.Items[0].MovieTitle.ShouldBe("Lorem Ipsum");
        response.Items[0].AuthorId.ShouldBe(AuthorId);
        response.Items[0].AuthorName.ShouldBe("Dolor Sit");
    }

    [Fact]
    public async Task ListReviews_ShouldReturn_OneReview()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Is<Queries.GetReviewById.GetReviewByIdQuery>(q => q.Id == ReviewId))
            .Returns(SampleReview());

        // Act
        var response = await ReviewTools.ListReviews(sender, CancellationToken.None, reviewId: ReviewId);

        // Assert
        response.TotalCount.ShouldBe(1);
        response.NextOffset.ShouldBeNull();
        response.Items.Single().Id.ShouldBe(ReviewId);
        _ = await sender.DidNotReceive().Send(Arg.Any<Queries.GetReviews.GetReviewsQuery>());
    }

    [Fact]
    public async Task WriteReview_ShouldCreate_Review()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Is<Commands.CreateReview.CreateReviewCommand>(c => c.MovieId == MovieId && c.AuthorId == AuthorId && c.Stars == 4))
            .Returns(SampleReview(4));

        // Act
        var response = await ReviewTools.WriteReview(sender, 4, CancellationToken.None, MovieId, AuthorId);

        // Assert
        response.Id.ShouldBe(ReviewId);
        response.Stars.ShouldBe(4);
        _ = await sender.DidNotReceive().Send(Arg.Any<Commands.UpdateReview.UpdateReviewCommand>());
    }

    [Fact]
    public async Task WriteReview_ShouldSend_EmptyIds_WhenMovieAndAuthorAreMissing()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Is<Commands.CreateReview.CreateReviewCommand>(c => c.MovieId == Guid.Empty && c.AuthorId == Guid.Empty && c.Stars == 4))
            .Returns(SampleReview(4));

        // Act
        var response = await ReviewTools.WriteReview(sender, 4, CancellationToken.None);

        // Assert
        response.Stars.ShouldBe(4);
        _ = await sender.Received(1).Send(Arg.Any<Commands.CreateReview.CreateReviewCommand>());
    }

    [Fact]
    public async Task WriteReview_ShouldUpdate_OnlyTheStars()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Is<Commands.UpdateReview.UpdateReviewCommand>(c => c.Id == ReviewId && c.Stars == 2))
            .Returns(true);

        _ = sender
            .Send(Arg.Is<Queries.GetReviewById.GetReviewByIdQuery>(q => q.Id == ReviewId))
            .Returns(SampleReview(2));

        // Act
        var response = await ReviewTools.WriteReview(sender, 2, CancellationToken.None, reviewId: ReviewId);

        // Assert
        response.Id.ShouldBe(ReviewId);
        response.Stars.ShouldBe(2);
        _ = await sender.DidNotReceive().Send(Arg.Any<Commands.CreateReview.CreateReviewCommand>());
    }

    [Fact]
    public async Task DeleteReview_ShouldReturn_Result()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Is<Commands.DeleteReview.DeleteReviewCommand>(c => c.Id == ReviewId))
            .Returns(true);

        // Act
        var response = await ReviewTools.DeleteReview(sender, ReviewId, CancellationToken.None);

        // Assert
        response.ReviewId.ShouldBe(ReviewId);
        response.Deleted.ShouldBeTrue();
    }
}
