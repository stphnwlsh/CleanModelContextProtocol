namespace CleanModelContextProtocol.Presentation.Resources;

using System.ComponentModel;
using CleanModelContextProtocol.Presentation.Models;
using CleanModelContextProtocol.Presentation.Serialization;
using Mediator;
using ModelContextProtocol.Server;
using Queries = Application.Reviews.Queries;

[McpServerResourceType]
public static class ReviewResources
{
    [McpServerResource(UriTemplate = "reviews://{reviewId}", Name = "review", Title = "Review", MimeType = "application/json")]
    [Description("A review with its star rating, the movie's title and the author's name.")]
    public static async Task<string> GetReview(
        ISender sender,
        [Description("The review's id (a GUID).")] Guid reviewId,
        CancellationToken cancellationToken)
    {
        var review = await sender.Send(new Queries.GetReviewById.GetReviewByIdQuery
        {
            Id = reviewId
        }, cancellationToken);

        return McpSerializer.Serialize(review.ToResult());
    }
}
