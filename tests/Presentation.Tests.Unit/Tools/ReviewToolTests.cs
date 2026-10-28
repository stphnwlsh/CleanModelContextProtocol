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
            .Send(Arg.Is<Queries.SearchReviews.SearchReviewsQuery>(q => q.MovieId == MovieId && q.AuthorId == AuthorId && q.Limit == 2 && q.Cursor == "abc"))
            .Returns(new Page<Entities.Review>([SampleReview()], 3, "next"));

        // Act
        var response = await ReviewTools.ListReviews(sender, MovieId, AuthorId, null, 2, "abc");

        // Assert
        response.TotalCount.ShouldBe(3);
        response.NextCursor.ShouldBe("next");
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
        var response = await ReviewTools.ListReviews(sender, reviewId: ReviewId);

        // Assert
        response.TotalCount.ShouldBe(1);
        response.NextCursor.ShouldBeNull();
        response.Items.Single().Id.ShouldBe(ReviewId);
        _ = await sender.DidNotReceive().Send(Arg.Any<Queries.SearchReviews.SearchReviewsQuery>());
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
        var response = await ReviewTools.WriteReview(sender, MovieId, AuthorId, 4);

        // Assert
        response.Id.ShouldBe(ReviewId);
        response.Stars.ShouldBe(4);
        _ = await sender.DidNotReceive().Send(Arg.Any<Commands.UpdateReview.UpdateReviewCommand>());
    }

    [Fact]
    public async Task WriteReview_ShouldUpdate_Review()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Is<Commands.UpdateReview.UpdateReviewCommand>(c => c.Id == ReviewId && c.MovieId == MovieId && c.AuthorId == AuthorId && c.Stars == 2))
            .Returns(true);

        _ = sender
            .Send(Arg.Is<Queries.GetReviewById.GetReviewByIdQuery>(q => q.Id == ReviewId))
            .Returns(SampleReview(2));

        // Act
        var response = await ReviewTools.WriteReview(sender, MovieId, AuthorId, 2, ReviewId);

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
        var response = await ReviewTools.DeleteReview(sender, ReviewId);

        // Assert
        response.ReviewId.ShouldBe(ReviewId);
        response.Deleted.ShouldBeTrue();
    }
}
