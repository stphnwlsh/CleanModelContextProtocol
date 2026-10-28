namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

using Application.Authors.Entities;
using Application.Common.Paging;
using Application.Reviews.Entities;
using CleanModelContextProtocol.Presentation.Tools;
using Mediator;
using NSubstitute;
using Shouldly;
using Xunit;
using Entities = Application.Movies.Entities;
using Queries = Application.Movies.Queries;

public class MovieToolTests
{
    private static readonly Guid MovieId = Guid.NewGuid();

    private static Entities.Movie SampleMovie()
    {
        return new(MovieId, "Lorem Ipsum",
        [
            new Review(Guid.NewGuid(), 5, null, new ReviewAuthor(Guid.NewGuid(), "Dolor", "Sit")),
            new Review(Guid.NewGuid(), 2, null, new ReviewAuthor(Guid.NewGuid(), "Amet", "Elit"))
        ]);
    }

    [Fact]
    public async Task SearchMovies_ShouldReturn_ConcisePage()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Is<Queries.GetMovies.GetMoviesQuery>(q => q.Query == "lorem" && q.Offset == 5 && q.Limit == 1))
            .Returns(new Page<Entities.Movie>([SampleMovie()], 5, 1, 7));

        // Act
        var response = await MovieTools.SearchMovies(sender, CancellationToken.None, "lorem", 5, 1);

        // Assert
        response.TotalCount.ShouldBe(7);
        response.NextOffset.ShouldBe(6);
        response.Items.Count.ShouldBe(1);
        response.Items[0].Id.ShouldBe(MovieId);
        response.Items[0].Title.ShouldBe("Lorem Ipsum");
        response.Items[0].ReviewCount.ShouldBe(2);
        response.Items[0].AverageStars.ShouldBe(3.5);
        response.Items[0].Reviews.ShouldBeNull();
    }

    [Fact]
    public async Task SearchMovies_ShouldReturn_Reviews()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetMovies.GetMoviesQuery>())
            .ReturnsForAnyArgs(new Page<Entities.Movie>([SampleMovie(), new Entities.Movie(Guid.Empty, "Empty")], 0, 10, 2));

        // Act
        var response = await MovieTools.SearchMovies(sender, CancellationToken.None, includeReviews: true);

        // Assert
        response.Items[0].Reviews.ShouldNotBeNull().Count.ShouldBe(2);
        response.Items[0].Reviews![0].MovieTitle.ShouldBe("Lorem Ipsum");
        response.Items[0].Reviews![0].AuthorName.ShouldBe("Dolor Sit");
        response.Items[1].ReviewCount.ShouldBe(0);
        response.Items[1].AverageStars.ShouldBeNull();
        response.Items[1].Reviews.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetMovie_ShouldReturn_MovieWithReviews()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Is<Queries.GetMovieById.GetMovieByIdQuery>(q => q.Id == MovieId))
            .Returns(SampleMovie());

        // Act
        var response = await MovieTools.GetMovie(sender, MovieId, CancellationToken.None);

        // Assert
        response.Id.ShouldBe(MovieId);
        response.AverageStars.ShouldBe(3.5);
        response.Reviews.ShouldNotBeNull().Count.ShouldBe(2);
        response.Reviews![1].Stars.ShouldBe(2);
        response.Reviews![1].MovieId.ShouldBe(MovieId);
        response.Reviews![1].AuthorName.ShouldBe("Amet Elit");
    }
}
