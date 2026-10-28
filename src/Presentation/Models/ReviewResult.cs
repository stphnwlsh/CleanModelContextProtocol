namespace CleanModelContextProtocol.Presentation.Models;

using System.ComponentModel;

/// <summary>
/// A review, with the names of the movie and author next to their ids.
/// </summary>
/// <param name="Id">The review's id.</param>
/// <param name="Stars">The star rating, from 1 (worst) to 5 (best).</param>
/// <param name="MovieId">The id of the movie that was reviewed.</param>
/// <param name="MovieTitle">The title of the movie that was reviewed.</param>
/// <param name="AuthorId">The id of the author who wrote the review.</param>
/// <param name="AuthorName">The full name of the author who wrote the review.</param>
public record ReviewResult(
    [property: Description("The review's id.")] Guid Id,
    [property: Description("The star rating, from 1 (worst) to 5 (best).")] int Stars,
    [property: Description("The id of the movie that was reviewed.")] Guid? MovieId,
    [property: Description("The title of the movie that was reviewed.")] string MovieTitle,
    [property: Description("The id of the author who wrote the review.")] Guid? AuthorId,
    [property: Description("The full name of the author who wrote the review.")] string AuthorName);
