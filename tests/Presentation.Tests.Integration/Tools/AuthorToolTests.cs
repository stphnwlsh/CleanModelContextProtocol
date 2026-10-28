namespace CleanModelContextProtocol.Presentation.Tests.Integration.Tools;

using CleanModelContextProtocol.Presentation.Models;
using Shouldly;
using Xunit;

public class AuthorToolTests : BaseToolTests
{
    [Fact]
    public async Task SearchAuthors_ShouldReturn_EveryAuthor()
    {
        // Act
        var page = await this.CallAsync<ResultPage<AuthorResult>>("authors_search", new Dictionary<string, object> { ["limit"] = 50 });

        // Assert
        page.TotalCount.ShouldBe(15);
        page.Items.Count.ShouldBe(15);
        page.NextCursor.ShouldBeNull();

        foreach (var author in page.Items)
        {
            author.Id.ShouldNotBe(Guid.Empty);
            author.Name.ShouldNotBeNullOrWhiteSpace();
            author.Reviews.ShouldBeNull();
        }
    }

    [Fact]
    public async Task SearchAuthors_ShouldFind_AuthorByFullName()
    {
        // Arrange
        var first = (await this.CallAsync<ResultPage<AuthorResult>>("authors_search")).Items[0];

        // Act
        var page = await this.CallAsync<ResultPage<AuthorResult>>("authors_search",
            new Dictionary<string, object> { ["query"] = first.Name.ToLowerInvariant(), ["includeReviews"] = true });

        // Assert
        var author = page.Items.ShouldHaveSingleItem();
        author.Id.ShouldBe(first.Id);
        author.Reviews.Count.ShouldBe(author.ReviewCount);
        author.Reviews.ShouldAllBe(r => r.AuthorId == author.Id && r.AuthorName == author.Name && r.MovieTitle != null);
    }

    [Fact]
    public async Task SearchAuthors_WithLongQuery_ShouldReturn_ValidationError()
    {
        // Act
        var text = await this.CallForErrorAsync("authors_search", new Dictionary<string, object> { ["query"] = new string('a', 101) });

        // Assert
        text.ShouldBe("Validation Failed: Query: A search term can't be longer than 100 characters. Correct the arguments and call the tool again.");
    }
}
