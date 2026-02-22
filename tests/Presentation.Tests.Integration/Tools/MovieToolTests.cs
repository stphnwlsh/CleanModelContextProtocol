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
        var result = await this.Client.CallToolAsync("GetAllMovies");
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
        var allResult = await this.Client.CallToolAsync("GetAllMovies");
        var allText = allResult.Content.OfType<TextContentBlock>().First().Text;
        var firstMovie = allText.Deserialize<List<Entities.Movie>>().First(m => m.Reviews?.Count > 0);

        // Act
        var result = await this.Client.CallToolAsync("GetMovieById",
            new Dictionary<string, object> { ["id"] = firstMovie.Id });
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

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("1")]
    [InlineData("fake")]
    public async Task GetMovieById_WithInvalidId_ShouldReturn_Error(string input)
    {
        // Valid-format GUIDs (including empty) pass as Guid; unparseable strings pass as string
        object idArg = Guid.TryParse(input, out var guid) ? guid : input;

        try
        {
            // Act
            var result = await this.Client.CallToolAsync("GetMovieById",
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
