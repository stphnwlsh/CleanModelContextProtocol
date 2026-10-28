namespace CleanModelContextProtocol.Presentation.Models;

using System.ComponentModel;

/// <summary>
/// A movie with a summary of how it was reviewed.
/// </summary>
/// <param name="Id">The movie's id.</param>
/// <param name="Title">The movie's title.</param>
/// <param name="ReviewCount">How many reviews the movie has.</param>
/// <param name="AverageStars">The average star rating, from 1 to 5, or null when there are no reviews.</param>
/// <param name="Reviews">Every review of the movie, when they were asked for.</param>
public record MovieResult(
    [property: Description("The movie's id.")] Guid Id,
    [property: Description("The movie's title.")] string Title,
    [property: Description("How many reviews the movie has.")] int ReviewCount,
    [property: Description("The average star rating, from 1 to 5, or null when there are no reviews.")] double? AverageStars,
    [property: Description("Every review of the movie, when they were asked for.")] List<ReviewResult>? Reviews = null);
