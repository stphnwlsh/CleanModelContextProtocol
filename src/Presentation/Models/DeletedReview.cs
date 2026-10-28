namespace CleanModelContextProtocol.Presentation.Models;

using System.ComponentModel;

/// <summary>
/// The result of deleting a review.
/// </summary>
/// <param name="ReviewId">The id of the review that was deleted.</param>
/// <param name="Deleted">Whether the review was deleted.</param>
public record DeletedReview(
    [property: Description("The id of the review that was deleted.")] Guid ReviewId,
    [property: Description("Whether the review was deleted.")] bool Deleted);
