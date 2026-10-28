namespace CleanModelContextProtocol.Presentation.Tests.Integration.Resources;

using CleanModelContextProtocol.Presentation.Models;
using Extensions;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;
using Entities = Application.Versions.Entities;

public class ResourceTests : BaseToolTests
{
    [Fact]
    public async Task ListResourceTemplates_ShouldReturn_EveryTemplate()
    {
        // Act
        var templates = await this.Client.ListResourceTemplatesAsync();

        // Assert
        templates.Select(t => t.UriTemplate).ShouldBe(["movies://{movieId}", "authors://{authorId}", "reviews://{reviewId}"], ignoreOrder: true);
        templates.ShouldAllBe(t => t.MimeType == "application/json" && !string.IsNullOrWhiteSpace(t.Title) && !string.IsNullOrWhiteSpace(t.Description));
    }

    [Fact]
    public async Task ListResources_ShouldReturn_ServerVersion()
    {
        // Act
        var resources = await this.Client.ListResourcesAsync();

        // Assert
        var resource = resources.ShouldHaveSingleItem();
        resource.Uri.ShouldBe("server://version");
        resource.MimeType.ShouldBe("application/json");
    }

    [Fact]
    public async Task ReadVersion_ShouldReturn_Version()
    {
        // Act
        var version = await this.ReadAsync<Entities.Version>("server://version");

        // Assert
        version.FileVersion.ShouldNotBeNullOrWhiteSpace();
        version.InformationalVersion.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ReadMovie_ShouldReturn_MovieWithReviews()
    {
        // Arrange
        var first = (await this.CallAsync<ResultPage<MovieResult>>("movies_search",
            new Dictionary<string, object> { ["limit"] = 50 })).Items.First(m => m.ReviewCount > 0);

        // Act
        var movie = await this.ReadAsync<MovieResult>($"movies://{first.Id}");

        // Assert
        movie.Id.ShouldBe(first.Id);
        movie.Title.ShouldBe(first.Title);
        movie.Reviews.Count.ShouldBe(first.ReviewCount);
    }

    [Fact]
    public async Task ReadAuthor_ShouldReturn_AuthorWithReviews()
    {
        // Arrange
        var first = (await this.CallAsync<ResultPage<AuthorResult>>("authors_search")).Items[0];

        // Act
        var author = await this.ReadAsync<AuthorResult>($"authors://{first.Id}");

        // Assert
        author.Id.ShouldBe(first.Id);
        author.Name.ShouldBe(first.Name);
        author.Reviews.Count.ShouldBe(first.ReviewCount);
    }

    [Fact]
    public async Task ReadReview_ShouldReturn_Review()
    {
        // Arrange
        var first = (await this.CallAsync<ResultPage<ReviewResult>>("reviews_list")).Items[0];

        // Act
        var review = await this.ReadAsync<ReviewResult>($"reviews://{first.Id}");

        // Assert
        review.ShouldBe(first);
    }

    [Fact]
    public async Task ReadMovie_WithUnknownId_ShouldThrow_ResourceNotFound()
    {
        // Act
        var exception = await Should.ThrowAsync<McpProtocolException>(() => this.Client.ReadResourceAsync($"movies://{Guid.NewGuid()}").AsTask());

        // Assert
        exception.ErrorCode.ShouldBe(McpErrorCode.ResourceNotFound);
        exception.Message.ShouldContain("The Movie with the supplied id was not found.");
    }

    [Fact]
    public async Task ReadAuthor_WithEmptyId_ShouldThrow_InvalidParams()
    {
        // Act
        var exception = await Should.ThrowAsync<McpProtocolException>(() => this.Client.ReadResourceAsync($"authors://{Guid.Empty}").AsTask());

        // Assert
        exception.ErrorCode.ShouldBe(McpErrorCode.InvalidParams);
        exception.Message.ShouldContain("An Author Id was not supplied.");
    }

    private async Task<T> ReadAsync<T>(string uri)
    {
        var result = await this.Client.ReadResourceAsync(uri);

        var contents = result.Contents.ShouldHaveSingleItem().ShouldBeOfType<TextResourceContents>();
        contents.Uri.ShouldBe(uri);
        contents.MimeType.ShouldBe("application/json");

        return contents.Text.Deserialize<T>();
    }
}
