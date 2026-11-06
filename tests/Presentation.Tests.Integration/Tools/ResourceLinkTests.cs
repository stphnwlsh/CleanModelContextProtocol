namespace CleanModelContextProtocol.Presentation.Tests.Integration.Tools;

using ModelContextProtocol.Protocol;
using Presentation.Models;
using Shouldly;
using Xunit;

public class ResourceLinkTests : BaseToolTests
{
    [Fact]
    public async Task GetMovie_ShouldLink_Movie()
    {
        // Arrange
        var movie = (await this.CallAsync<ResultPage<MovieResult>>("movies_search")).Items[0];

        // Act
        var links = await this.GetLinksAsync("movies_get", new Dictionary<string, object?> { ["movieId"] = movie.Id });

        // Assert
        var link = links.ShouldHaveSingleItem();

        link.Uri.ShouldBe($"movies://{movie.Id}");
        link.Name.ShouldBe(movie.Title);
        link.MimeType.ShouldBe("application/json");
        await this.ShouldResolveAsync(links);
    }

    [Fact]
    public async Task SearchMovies_ShouldLink_EveryMovie()
    {
        // Act
        var links = await this.GetLinksAsync("movies_search");

        // Assert
        links.Count.ShouldBe(10);
        links.ShouldAllBe(l => l.Uri.StartsWith("movies://", StringComparison.Ordinal));
        await this.ShouldResolveAsync(links);
    }

    [Fact]
    public async Task SearchAuthors_ShouldLink_EveryAuthor()
    {
        // Act
        var links = await this.GetLinksAsync("authors_search");

        // Assert
        links.Count.ShouldBe(10);
        links.ShouldAllBe(l => l.Uri.StartsWith("authors://", StringComparison.Ordinal));
        await this.ShouldResolveAsync(links);
    }

    [Fact]
    public async Task ListReviews_ShouldLink_EveryReview()
    {
        // Act
        var links = await this.GetLinksAsync("reviews_list");

        // Assert
        links.Count.ShouldBe(10);
        links.ShouldAllBe(l => l.Uri.StartsWith("reviews://", StringComparison.Ordinal));
        links.ShouldAllBe(l => l.Name.StartsWith("Review of ", StringComparison.Ordinal));
        await this.ShouldResolveAsync(links);
    }

    [Fact]
    public async Task WriteReview_ShouldLink_Review()
    {
        // Arrange
        var movie = (await this.CallAsync<ResultPage<MovieResult>>("movies_search")).Items[0];
        var author = (await this.CallAsync<ResultPage<AuthorResult>>("authors_search")).Items[0];

        // Act
        var links = await this.GetLinksAsync("reviews_write",
            new Dictionary<string, object?> { ["movieId"] = movie.Id, ["authorId"] = author.Id, ["stars"] = 5 });

        // Assert
        var link = links.ShouldHaveSingleItem();

        link.Name.ShouldBe($"Review of {movie.Title} by {author.Name}");
        await this.ShouldResolveAsync(links);
    }

    [Fact]
    public async Task DeleteReview_ShouldNotLink_DeletedReview()
    {
        // Arrange
        var review = (await this.CallAsync<ResultPage<ReviewResult>>("reviews_list")).Items[0];

        // Act
        var links = await this.GetLinksAsync("reviews_delete", new Dictionary<string, object?> { ["reviewId"] = review.Id });

        // Assert
        links.ShouldBeEmpty();
    }

    [Fact]
    public async Task SearchMovies_ShouldKeep_StructuredContentAndText()
    {
        // Act
        var result = await this.Client.CallToolAsync("movies_search");

        // Assert
        result.StructuredContent.ShouldNotBeNull().GetProperty("items").GetArrayLength().ShouldBe(10);
        _ = result.Content[0].ShouldBeOfType<TextContentBlock>();
    }

    [Fact]
    public async Task GetMovie_NotFound_ShouldNotLink()
    {
        // Act
        var result = await this.Client.CallToolAsync("movies_get", new Dictionary<string, object?> { ["movieId"] = Guid.NewGuid() });

        // Assert
        result.IsError.ShouldBe(true);
        result.Content.OfType<ResourceLinkBlock>().ShouldBeEmpty();
    }

    private async Task<List<ResourceLinkBlock>> GetLinksAsync(string name, Dictionary<string, object?>? arguments = null)
    {
        var result = await this.Client.CallToolAsync(name, arguments);

        result.IsError.ShouldNotBe(true);

        return [.. result.Content.OfType<ResourceLinkBlock>()];
    }

    private async Task ShouldResolveAsync(List<ResourceLinkBlock> links)
    {
        foreach (var link in links)
        {
            var resource = await this.Client.ReadResourceAsync(link.Uri);

            resource.Contents.ShouldNotBeEmpty();
        }
    }
}
