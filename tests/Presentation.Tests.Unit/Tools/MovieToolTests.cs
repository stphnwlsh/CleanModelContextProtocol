namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

using System.Text.Json;
using CleanModelContextProtocol.Application.Common.Exceptions;
using CleanModelContextProtocol.Presentation.Tools;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
        var response = await MovieTools.GetAllMovies(sender);

        // Assert
        var result = JsonSerializer.Deserialize<List<Entities.Movie>>(response, JsonOptions);

        result.ShouldNotBeNull();
        result[0].Id.ShouldBe(Guid.Empty);
        result[0].Title.ShouldBe("Lorem Ipsum");
        result[0].Reviews.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllMovies_ShouldReturn_Error()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetMovies.GetMoviesQuery>())
            .Throws(new ArgumentException("Expected Exception"));

        // Act
        var response = await MovieTools.GetAllMovies(sender);

        // Assert
        response.ShouldBe("Error: Expected Exception");
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
        var response = await MovieTools.GetMovieById(sender, Guid.Empty);

        // Assert
        var result = JsonSerializer.Deserialize<Entities.Movie>(response, JsonOptions);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(Guid.Empty);
        result.Title.ShouldBe("Lorem Ipsum");
        result.Reviews.ShouldBeNull();
    }

    [Fact]
    public async Task GetMovieById_ShouldReturn_NotFound()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetMovieById.GetMovieByIdQuery>())
            .Throws(new NotFoundException("Expected Exception"));

        // Act
        var response = await MovieTools.GetMovieById(sender, Guid.Empty);

        // Assert
        response.ShouldBe("Not Found: Expected Exception");
    }

    [Fact]
    public async Task GetMovieById_ShouldReturn_Error()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetMovieById.GetMovieByIdQuery>())
            .Throws(new ArgumentException("Expected Exception"));

        // Act
        var response = await MovieTools.GetMovieById(sender, Guid.Empty);

        // Assert
        response.ShouldBe("Error: Expected Exception");
    }
}
