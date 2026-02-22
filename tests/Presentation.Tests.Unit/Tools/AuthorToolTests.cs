namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

using System.Text.Json;
using CleanModelContextProtocol.Application.Common.Exceptions;
using CleanModelContextProtocol.Presentation.Tools;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;
using Entities = Application.Authors.Entities;
using Queries = Application.Authors.Queries;

public class AuthorToolTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task GetAllAuthors_ShouldReturn_JsonResult()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetAuthors.GetAuthorsQuery>())
            .ReturnsForAnyArgs(
            [
                new Entities.Author(Guid.Empty, "Lorem", "Ipsum")
            ]);

        // Act
        var response = await AuthorTools.GetAllAuthors(sender);

        // Assert
        var result = JsonSerializer.Deserialize<List<Entities.Author>>(response, JsonOptions);

        result.ShouldNotBeNull();
        result[0].Id.ShouldBe(Guid.Empty);
        result[0].FirstName.ShouldBe("Lorem");
        result[0].LastName.ShouldBe("Ipsum");
        result[0].Reviews.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAuthors_ShouldReturn_Error()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetAuthors.GetAuthorsQuery>())
            .Throws(new ArgumentException("Expected Exception"));

        // Act
        var response = await AuthorTools.GetAllAuthors(sender);

        // Assert
        response.ShouldBe("Error: Expected Exception");
    }

    [Fact]
    public async Task GetAuthorById_ShouldReturn_JsonResult()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetAuthorById.GetAuthorByIdQuery>())
            .ReturnsForAnyArgs(new Entities.Author(Guid.Empty, "Lorem", "Ipsum"));

        // Act
        var response = await AuthorTools.GetAuthorById(sender, Guid.Empty);

        // Assert
        var result = JsonSerializer.Deserialize<Entities.Author>(response, JsonOptions);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(Guid.Empty);
        result.FirstName.ShouldBe("Lorem");
        result.LastName.ShouldBe("Ipsum");
        result.Reviews.ShouldBeNull();
    }

    [Fact]
    public async Task GetAuthorById_ShouldReturn_NotFound()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetAuthorById.GetAuthorByIdQuery>())
            .Throws(new NotFoundException("Expected Exception"));

        // Act
        var response = await AuthorTools.GetAuthorById(sender, Guid.Empty);

        // Assert
        response.ShouldBe("Not Found: Expected Exception");
    }

    [Fact]
    public async Task GetAuthorById_ShouldReturn_Error()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetAuthorById.GetAuthorByIdQuery>())
            .Throws(new ArgumentException("Expected Exception"));

        // Act
        var response = await AuthorTools.GetAuthorById(sender, Guid.Empty);

        // Assert
        response.ShouldBe("Error: Expected Exception");
    }
}
