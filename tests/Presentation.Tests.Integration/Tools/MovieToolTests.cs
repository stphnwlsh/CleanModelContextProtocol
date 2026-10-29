namespace CleanModelContextProtocol.Presentation.Tests.Integration.Tools;

using CleanModelContextProtocol.Presentation.Models;
using ModelContextProtocol;
using Shouldly;
using Xunit;

public class MovieToolTests : BaseToolTests
{
    [Fact]
    public async Task SearchMovies_ShouldReturn_FirstPage()
    {
        // Act
        var page = await this.CallAsync<ResultPage<MovieResult>>("movies_search");

        // Assert
        page.TotalCount.ShouldBe(50);
        page.Items.Count.ShouldBe(10);
        page.NextOffset.ShouldBe(10);
        page.Items.Select(m => m.Title).ShouldBeInOrder(SortDirection.Ascending, StringComparer.CurrentCulture);

        foreach (var movie in page.Items)
        {
            movie.Id.ShouldNotBe(Guid.Empty);
            movie.Title.ShouldNotBeNullOrWhiteSpace();
            movie.Reviews.ShouldBeNull();

            if (movie.ReviewCount > 0)
            {
                movie.AverageStars.ShouldNotBeNull().ShouldBeInRange(1, 5);
            }
        }
    }

    [Fact]
    public async Task SearchMovies_ShouldPage_ThroughEveryMovie()
    {
        // Arrange
        var titles = new List<string>();
        int? offset = 0;

        // Act
        do
        {
            var arguments = new Dictionary<string, object?> { ["limit"] = 20, ["offset"] = offset };

            var page = await this.CallAsync<ResultPage<MovieResult>>("movies_search", arguments);

            titles.AddRange(page.Items.Select(m => m.Title));
            offset = page.NextOffset;
        }
        while (offset is not null);

        // Assert
        titles.Count.ShouldBe(50);
    }

    [Fact]
    public async Task SearchMovies_ShouldFind_MovieByTitle()
    {
        // Arrange
        var first = (await this.CallAsync<ResultPage<MovieResult>>("movies_search")).Items[0];

        // Act
        var page = await this.CallAsync<ResultPage<MovieResult>>("movies_search",
            new Dictionary<string, object?> { ["query"] = first.Title.ToUpperInvariant(), ["includeReviews"] = true });

        // Assert
        page.Items.ShouldContain(m => m.Id == first.Id);
        page.Items.ShouldAllBe(m => m.Title.Contains(first.Title, StringComparison.OrdinalIgnoreCase));
        page.Items.ShouldAllBe(m => m.Reviews != null && m.Reviews!.Count == m.ReviewCount);
    }

    [Fact]
    public async Task SearchMovies_WithInvalidLimit_ShouldReturn_ValidationError()
    {
        // Act
        var text = await this.CallForErrorAsync("movies_search", new Dictionary<string, object?> { ["limit"] = 500 });

        // Assert
        text.ShouldBe("Validation Failed: Limit: A limit must be between 1 and 50. Correct the arguments and call the tool again.");
    }

    [Fact]
    public async Task SearchMovies_WithNegativeOffset_ShouldReturn_ValidationError()
    {
        // Act
        var text = await this.CallForErrorAsync("movies_search", new Dictionary<string, object?> { ["offset"] = -1 });

        // Assert
        text.ShouldStartWith("Validation Failed: Offset: An offset can't be negative.");
    }

    [Fact]
    public async Task GetMovie_ShouldReturn_MovieWithReviews()
    {
        // Arrange
        var first = (await this.CallAsync<ResultPage<MovieResult>>("movies_search",
            new Dictionary<string, object?> { ["limit"] = 50 })).Items.First(m => m.ReviewCount > 0);

        // Act
        var movie = await this.CallAsync<MovieResult>("movies_get", new Dictionary<string, object?> { ["movieId"] = first.Id });

        // Assert
        movie.Id.ShouldBe(first.Id);
        movie.Title.ShouldBe(first.Title);
        movie.ReviewCount.ShouldBe(first.ReviewCount);
        movie.AverageStars.ShouldBe(first.AverageStars);
        movie.Reviews.ShouldNotBeNull().Count.ShouldBe(movie.ReviewCount);

        foreach (var review in movie.Reviews.ShouldNotBeNull())
        {
            review.Stars.ShouldBeInRange(1, 5);
            review.MovieId.ShouldBe(movie.Id);
            review.MovieTitle.ShouldBe(movie.Title);
            review.AuthorId.ShouldNotBeNull();
            review.AuthorName.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetMovie_WithEmptyId_ShouldReturn_ValidationError()
    {
        // Act
        var text = await this.CallForErrorAsync("movies_get", new Dictionary<string, object?> { ["movieId"] = Guid.Empty });

        // Assert
        text.ShouldBe("Validation Failed: Id: A Movie Id was not supplied. Correct the arguments and call the tool again.");
    }

    [Fact]
    public async Task GetMovie_WithUnknownId_ShouldReturn_NotFoundError()
    {
        // Act
        var text = await this.CallForErrorAsync("movies_get", new Dictionary<string, object?> { ["movieId"] = Guid.NewGuid() });

        // Assert
        text.ShouldBe("Not Found: The Movie with the supplied id was not found. Check the ids: movies_search, authors_search and reviews_list return valid ones.");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("fake")]
    public async Task GetMovie_WithInvalidId_ShouldReturn_Error(string input)
    {
        try
        {
            // Act
            var result = await this.Client.CallToolAsync("movies_get",
                new Dictionary<string, object?> { ["movieId"] = input });

            // Assert
            result.IsError.ShouldBe(true);
        }
        catch (McpException)
        {
            // Protocol-level error for unparseable inputs - expected
        }
    }
}
