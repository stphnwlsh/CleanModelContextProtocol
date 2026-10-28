namespace CleanModelContextProtocol.Presentation.Models;

using System.ComponentModel;

/// <summary>
/// An author with a summary of the reviews they have written.
/// </summary>
/// <param name="Id">The author's id.</param>
/// <param name="Name">The author's full name.</param>
/// <param name="ReviewCount">How many reviews the author has written.</param>
/// <param name="AverageStars">The average star rating the author gives, from 1 to 5, or null when they have no reviews.</param>
/// <param name="Reviews">Every review the author has written, when they were asked for.</param>
public record AuthorResult(
    [property: Description("The author's id.")] Guid Id,
    [property: Description("The author's full name.")] string Name,
    [property: Description("How many reviews the author has written.")] int ReviewCount,
    [property: Description("The average star rating the author gives, from 1 to 5, or null when they have no reviews.")] double? AverageStars,
    [property: Description("Every review the author has written, when they were asked for.")] List<ReviewResult>? Reviews = null);
