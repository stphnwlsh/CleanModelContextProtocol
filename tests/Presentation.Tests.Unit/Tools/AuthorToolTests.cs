namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

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
    public async Task GetAllAuthors_ShouldReturn_Result()
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
        var response = await AuthorTools.ListAuthors(sender);

        // Assert
        var result = response;

        result.ShouldNotBeNull();
        result[0].Id.ShouldBe(Guid.Empty);
        result[0].FirstName.ShouldBe("Lorem");
        result[0].LastName.ShouldBe("Ipsum");
        result[0].Reviews.ShouldBeNull();
    }

    [Fact]
    public async Task GetAuthorById_ShouldReturn_Result()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetAuthorById.GetAuthorByIdQuery>())
            .ReturnsForAnyArgs(new Entities.Author(Guid.Empty, "Lorem", "Ipsum"));

        // Act
        var response = await AuthorTools.GetAuthor(sender, Guid.Empty);

        // Assert
        var result = response;

        result.ShouldNotBeNull();
        result.Id.ShouldBe(Guid.Empty);
        result.FirstName.ShouldBe("Lorem");
        result.LastName.ShouldBe("Ipsum");
        result.Reviews.ShouldBeNull();
    }
}
