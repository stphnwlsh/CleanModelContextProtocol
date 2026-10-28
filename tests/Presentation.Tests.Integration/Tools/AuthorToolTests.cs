namespace CleanModelContextProtocol.Presentation.Tests.Integration.Tools;

using Extensions;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;
using Entities = Application.Authors.Entities;

public class AuthorToolTests : BaseToolTests
{
    [Fact]
    public async Task GetAllAuthors_ShouldReturn_Authors()
    {
        // Act
        var result = await this.Client.CallToolAsync("authors_list");
        var text = result.Content.OfType<TextContentBlock>().First().Text;
        var authors = text.Deserialize<List<Entities.Author>>();

        // Assert
        _ = authors.ShouldNotBeNull();

        authors.ShouldNotBeEmpty();
        authors.Count.ShouldBe(15);

        foreach (var author in authors)
        {
            _ = author.ShouldNotBeNull();
            _ = author.Id.ShouldBeOfType<Guid>();
            _ = author.FirstName.ShouldBeOfType<string>();
            author.FirstName.ShouldNotBeNullOrWhiteSpace();
            _ = author.LastName.ShouldBeOfType<string>();
            author.LastName.ShouldNotBeNullOrWhiteSpace();

            foreach (var review in author.Reviews ?? [])
            {
                _ = review.Stars.ShouldBeOfType<int>();
                review.Stars.ShouldBeInRange(1, 5);
                _ = review.ReviewedMovie.ShouldNotBeNull();
                _ = review.ReviewedMovie.Id.ShouldBeOfType<Guid>();
                _ = review.ReviewedMovie.Title.ShouldBeOfType<string>();
                review.ReviewedMovie.Title.ShouldNotBeNullOrWhiteSpace();
            }
        }
    }

    [Fact]
    public async Task GetAuthorById_ShouldReturn_Author()
    {
        // Arrange
        var allResult = await this.Client.CallToolAsync("authors_list");
        var allText = allResult.Content.OfType<TextContentBlock>().First().Text;
        var firstAuthor = allText.Deserialize<List<Entities.Author>>()[0];

        // Act
        var result = await this.Client.CallToolAsync("authors_get",
            new Dictionary<string, object> { ["authorId"] = firstAuthor.Id });
        var text = result.Content.OfType<TextContentBlock>().First().Text;
        var author = text.Deserialize<Entities.Author>();

        // Assert
        _ = author.ShouldNotBeNull();

        author.Id.ShouldBe(firstAuthor.Id);
        _ = author.FirstName.ShouldBeOfType<string>();
        author.FirstName.ShouldNotBeNullOrWhiteSpace();
        _ = author.LastName.ShouldBeOfType<string>();
        author.LastName.ShouldNotBeNullOrWhiteSpace();
        author.Reviews.ShouldNotBeEmpty();

        foreach (var review in author.Reviews ?? [])
        {
            _ = review.Stars.ShouldBeOfType<int>();
            review.Stars.ShouldBeInRange(1, 5);
            _ = review.ReviewedMovie.ShouldNotBeNull();
            _ = review.ReviewedMovie!.Id.ShouldBeOfType<Guid>();
            _ = review.ReviewedMovie.Title.ShouldBeOfType<string>();
            review.ReviewedMovie.Title.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetAuthorById_WithEmptyId_ShouldReturn_ValidationError()
    {
        // Act
        var result = await this.Client.CallToolAsync("authors_get",
            new Dictionary<string, object> { ["authorId"] = Guid.Empty });
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Assert
        result.IsError.ShouldBe(true);
        text.ShouldBe("Validation Failed: Id: An Author Id was not supplied. Correct the arguments and call the tool again.");
    }

    [Fact]
    public async Task GetAuthorById_WithUnknownId_ShouldReturn_NotFoundError()
    {
        // Act
        var result = await this.Client.CallToolAsync("authors_get",
            new Dictionary<string, object> { ["authorId"] = Guid.NewGuid() });
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Assert
        result.IsError.ShouldBe(true);
        text.ShouldBe("Not Found: The Author with the supplied id was not found. Check the ids, using authors_list or the other list tools to find valid ones.");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("fake")]
    public async Task GetAuthorById_WithInvalidId_ShouldReturn_Error(string input)
    {
        try
        {
            // Act
            var result = await this.Client.CallToolAsync("authors_get",
                new Dictionary<string, object> { ["authorId"] = input });

            // Assert
            result.IsError.ShouldBe(true);
        }
        catch (McpException)
        {
            // Protocol-level error for unparseable inputs - expected
        }
    }
}
