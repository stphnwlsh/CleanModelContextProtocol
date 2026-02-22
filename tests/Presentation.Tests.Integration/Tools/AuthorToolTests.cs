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
        var result = await this.Client.CallToolAsync("GetAllAuthors");
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
        var allResult = await this.Client.CallToolAsync("GetAllAuthors");
        var allText = allResult.Content.OfType<TextContentBlock>().First().Text;
        var firstAuthor = allText.Deserialize<List<Entities.Author>>()[0];

        // Act
        var result = await this.Client.CallToolAsync("GetAuthorById",
            new Dictionary<string, object> { ["id"] = firstAuthor.Id });
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

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("1")]
    [InlineData("fake")]
    public async Task GetAuthorById_WithInvalidId_ShouldReturn_Error(string input)
    {
        // Valid-format GUIDs (including empty) pass as Guid; unparseable strings pass as string
        object idArg = Guid.TryParse(input, out var guid) ? guid : input;

        try
        {
            // Act
            var result = await this.Client.CallToolAsync("GetAuthorById",
                new Dictionary<string, object> { ["id"] = idArg });

            // Assert - empty GUID returns "Not Found:" text; binding errors may set IsError
            var isError = result.IsError == true;
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text ?? string.Empty;
            var isTextError = textContent.StartsWith("Not Found:", StringComparison.Ordinal)
                || textContent.StartsWith("Error:", StringComparison.Ordinal);

            (isError || isTextError).ShouldBeTrue();
        }
        catch (McpException)
        {
            // Protocol-level error for unparseable inputs - expected
        }
    }
}
