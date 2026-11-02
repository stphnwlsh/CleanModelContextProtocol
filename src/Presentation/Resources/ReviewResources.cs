namespace CleanModelContextProtocol.Presentation.Resources;

using CleanModelContextProtocol.Presentation.Models;
using CleanModelContextProtocol.Presentation.Serialization;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Reviews.Queries;

[McpServerResourceType]
public static partial class ReviewResources
{
    /// <summary>A review with its star rating, the movie's title and the author's name.</summary>
    /// <param name="reviewId">The review's id (a GUID).</param>
    [McpServerResource(UriTemplate = "reviews://{reviewId}", Name = "review", Title = "Review", MimeType = "application/json")]
    public static async partial Task<string> GetReview(
        ISender sender,
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        var review = await sender.Send(new Queries.GetReviewById.GetReviewByIdQuery
        {
            Id = reviewId
        }, cancellationToken);

        return McpSerializer.Serialize(review.ToResult());
    }
}
