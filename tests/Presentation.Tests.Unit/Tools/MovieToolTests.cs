namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

using CleanModelContextProtocol.Presentation.Tools;
using Mediator;
using NSubstitute;
using Shouldly;
using Xunit;
using Entities = Application.Movies.Entities;
using Queries = Application.Movies.Queries;

public class MovieToolTests
{
    [Fact]
    public async Task GetAllMovies_ShouldReturn_Result()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetMovies.GetMoviesQuery>())
            .ReturnsForAnyArgs(
            [
                new Entities.Movie(Guid.Empty, "Lorem Ipsum")
            ]);

        // Act
        var response = await MovieTools.ListMovies(sender);

        // Assert
        var result = response;

        result.ShouldNotBeNull();
        result[0].Id.ShouldBe(Guid.Empty);
        result[0].Title.ShouldBe("Lorem Ipsum");
        result[0].Reviews.ShouldBeNull();
    }

    [Fact]
    public async Task GetMovieById_ShouldReturn_Result()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetMovieById.GetMovieByIdQuery>())
            .ReturnsForAnyArgs(new Entities.Movie(Guid.Empty, "Lorem Ipsum"));

        // Act
        var response = await MovieTools.GetMovie(sender, Guid.Empty);

        // Assert
        var result = response;

        result.ShouldNotBeNull();
        result.Id.ShouldBe(Guid.Empty);
        result.Title.ShouldBe("Lorem Ipsum");
        result.Reviews.ShouldBeNull();
    }
}
