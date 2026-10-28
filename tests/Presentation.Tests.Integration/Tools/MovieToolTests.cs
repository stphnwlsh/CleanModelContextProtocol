namespace CleanModelContextProtocol.Presentation.Tests.Integration.Tools;

using Extensions;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;
using Entities = Application.Movies.Entities;

public class MovieToolTests : BaseToolTests
{
    [Fact]
    public async Task GetAllMovies_ShouldReturn_Movies()
    {
        // Act
        var result = await this.Client.CallToolAsync("movies_list");
        var text = result.Content.OfType<TextContentBlock>().First().Text;
        var movies = text.Deserialize<List<Entities.Movie>>();

        // Assert
        _ = movies.ShouldNotBeNull();

        movies.ShouldNotBeEmpty();
        movies.Count.ShouldBe(50);

        foreach (var movie in movies)
        {
            _ = movie.ShouldNotBeNull();
            _ = movie.Id.ShouldBeOfType<Guid>();
            _ = movie.Title.ShouldBeOfType<string>();
            movie.Title.ShouldNotBeNullOrWhiteSpace();

            foreach (var review in movie.Reviews ?? [])
            {
                _ = review.Stars.ShouldBeOfType<int>();
                review.Stars.ShouldBeInRange(1, 5);

                _ = review.ReviewAuthor.ShouldNotBeNull();
                _ = review.ReviewAuthor.Id.ShouldBeOfType<Guid>();
                _ = review.ReviewAuthor.FirstName.ShouldBeOfType<string>();
                review.ReviewAuthor.FirstName.ShouldNotBeNullOrWhiteSpace();
                _ = review.ReviewAuthor.LastName.ShouldBeOfType<string>();
                review.ReviewAuthor.LastName.ShouldNotBeNullOrWhiteSpace();
            }
        }
    }

    [Fact]
    public async Task GetMovieById_ShouldReturn_Movie()
    {
        // Arrange
        var allResult = await this.Client.CallToolAsync("movies_list");
        var allText = allResult.Content.OfType<TextContentBlock>().First().Text;
        var firstMovie = allText.Deserialize<List<Entities.Movie>>().First(m => m.Reviews?.Count > 0);

        // Act
        var result = await this.Client.CallToolAsync("movies_get",
            new Dictionary<string, object> { ["movieId"] = firstMovie.Id });
        var text = result.Content.OfType<TextContentBlock>().First().Text;
        var movie = text.Deserialize<Entities.Movie>();

        // Assert
        _ = movie.ShouldNotBeNull();

        movie.Id.ShouldBe(firstMovie.Id);
        _ = movie.Title.ShouldBeOfType<string>();
        movie.Title.ShouldNotBeNullOrWhiteSpace();
        movie.Reviews.ShouldNotBeEmpty();

        foreach (var review in movie.Reviews ?? [])
        {
            _ = review.Stars.ShouldBeOfType<int>();
            review.Stars.ShouldBeInRange(1, 5);

            _ = review.ReviewAuthor.ShouldNotBeNull();
            _ = review.ReviewAuthor.Id.ShouldBeOfType<Guid>();
            _ = review.ReviewAuthor.FirstName.ShouldBeOfType<string>();
            review.ReviewAuthor.FirstName.ShouldNotBeNullOrWhiteSpace();
            _ = review.ReviewAuthor.LastName.ShouldBeOfType<string>();
            review.ReviewAuthor.LastName.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetMovieById_WithEmptyId_ShouldReturn_ValidationError()
    {
        // Act
        var result = await this.Client.CallToolAsync("movies_get",
            new Dictionary<string, object> { ["movieId"] = Guid.Empty });
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Assert
        result.IsError.ShouldBe(true);
        text.ShouldBe("Validation Failed: Id: A Movie Id was not supplied. Correct the arguments and call the tool again.");
    }

    [Fact]
    public async Task GetMovieById_WithUnknownId_ShouldReturn_NotFoundError()
    {
        // Act
        var result = await this.Client.CallToolAsync("movies_get",
            new Dictionary<string, object> { ["movieId"] = Guid.NewGuid() });
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Assert
        result.IsError.ShouldBe(true);
        text.ShouldBe("Not Found: The Movie with the supplied id was not found. Check the ids, using movies_list or the other list tools to find valid ones.");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("fake")]
    public async Task GetMovieById_WithInvalidId_ShouldReturn_Error(string input)
    {
        try
        {
            // Act
            var result = await this.Client.CallToolAsync("movies_get",
                new Dictionary<string, object> { ["movieId"] = input });

            // Assert
            result.IsError.ShouldBe(true);
        }
        catch (McpException)
        {
            // Protocol-level error for unparseable inputs - expected
        }
    }
}
