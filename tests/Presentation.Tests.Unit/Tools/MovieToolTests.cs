namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

using System.Text.Json;
using CleanModelContextProtocol.Presentation.Tools;
using Mediator;
using NSubstitute;
using Shouldly;
using Xunit;
using Entities = Application.Movies.Entities;
using Queries = Application.Movies.Queries;

public class MovieToolTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task GetAllMovies_ShouldReturn_JsonResult()
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
        var result = JsonSerializer.Deserialize<List<Entities.Movie>>(response, JsonOptions);

        result.ShouldNotBeNull();
        result[0].Id.ShouldBe(Guid.Empty);
        result[0].Title.ShouldBe("Lorem Ipsum");
        result[0].Reviews.ShouldBeNull();
    }

    [Fact]
    public async Task GetMovieById_ShouldReturn_JsonResult()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetMovieById.GetMovieByIdQuery>())
            .ReturnsForAnyArgs(new Entities.Movie(Guid.Empty, "Lorem Ipsum"));

        // Act
        var response = await MovieTools.GetMovie(sender, Guid.Empty);

        // Assert
        var result = JsonSerializer.Deserialize<Entities.Movie>(response, JsonOptions);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(Guid.Empty);
        result.Title.ShouldBe("Lorem Ipsum");
        result.Reviews.ShouldBeNull();
    }
}
