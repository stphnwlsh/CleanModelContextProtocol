namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

using Application.Common.Paging;
using Application.Movies.Entities;
using Application.Reviews.Entities;
using CleanModelContextProtocol.Presentation.Tools;
using Mediator;
using NSubstitute;
using Shouldly;
using Xunit;
using Entities = Application.Authors.Entities;
using Queries = Application.Authors.Queries;

public class AuthorToolTests
{
    [Fact]
    public async Task SearchAuthors_ShouldReturn_ConcisePage()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var author = new Entities.AuthorSummary(Guid.Empty, "Lorem", "Ipsum", 1, 4);

        _ = sender
            .Send(Arg.Is<Queries.GetAuthors.GetAuthorsQuery>(q => q.Query == "lorem" && q.Offset == 0 && q.Limit == 10))
            .Returns(new Page<Entities.AuthorSummary>([author], 0, 10, 1));

        // Act
        var response = await AuthorTools.SearchAuthors(sender, CancellationToken.None, "lorem");

        // Assert
        response.TotalCount.ShouldBe(1);
        response.NextOffset.ShouldBeNull();
        response.Items[0].Name.ShouldBe("Lorem Ipsum");
        response.Items[0].ReviewCount.ShouldBe(1);
        response.Items[0].AverageStars.ShouldBe(4);
        response.Items[0].Reviews.ShouldBeNull();
    }

    [Fact]
    public async Task SearchAuthors_ShouldReturn_Reviews()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var author = new Entities.AuthorSummary(Guid.Empty, "Lorem", "Ipsum", 1, 4);

        _ = sender
            .Send(Arg.Any<Queries.GetAuthors.GetAuthorsQuery>())
            .ReturnsForAnyArgs(new Page<Entities.AuthorSummary>([author], 0, 10, 1));
        _ = sender
            .Send(Arg.Is<Application.Reviews.Queries.GetReviewsForAuthors.GetReviewsForAuthorsQuery>(q => q.AuthorIds.Single() == Guid.Empty))
            .Returns(new Dictionary<Guid, List<Review>> { [Guid.Empty] = [new Review(Guid.Empty, 4, new ReviewedMovie(Guid.Empty, "Dolor"))] });

        // Act
        var response = await AuthorTools.SearchAuthors(sender, CancellationToken.None, includeReviews: true);

        // Assert
        response.Items[0].Reviews.ShouldNotBeNull().Count.ShouldBe(1);
        response.Items[0].Reviews![0].MovieTitle.ShouldBe("Dolor");
        response.Items[0].Reviews![0].AuthorName.ShouldBe("Lorem Ipsum");
    }

    [Fact]
    public async Task SearchAuthors_ShouldSkip_Reviews_WhenPageIsEmpty()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetAuthors.GetAuthorsQuery>())
            .ReturnsForAnyArgs(new Page<Entities.AuthorSummary>([], 0, 10, 0));

        // Act
        var response = await AuthorTools.SearchAuthors(sender, CancellationToken.None, "missing", includeReviews: true);

        // Assert
        response.Items.ShouldBeEmpty();
        _ = await sender.DidNotReceive().Send(Arg.Any<Application.Reviews.Queries.GetReviewsForAuthors.GetReviewsForAuthorsQuery>(), Arg.Any<CancellationToken>());
    }
}
