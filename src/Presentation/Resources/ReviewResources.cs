namespace CleanModelContextProtocol.Presentation.Resources;

using Mediator;
using ModelContextProtocol.Server;
using Presentation.Models;
using Presentation.Serialization;
using Presentation.Tools;
using Queries = Application.Reviews.Queries;

[McpServerResourceType]
public static partial class ReviewResources
{
    /// <summary>A review with its star rating, the movie's title and the author's name.</summary>
    /// <param name="reviewId">The review's id (a GUID).</param>
    [McpServerResource(UriTemplate = "reviews://{reviewId}", Name = "review", Title = "Review", MimeType = "application/json", IconSource = ItemIcons.Review)]
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
