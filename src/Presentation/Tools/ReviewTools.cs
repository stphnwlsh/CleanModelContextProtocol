namespace CleanModelContextProtocol.Presentation.Tools;

using Mediator;
using ModelContextProtocol.Server;
using Presentation.Models;
using Commands = Application.Reviews.Commands;
using Queries = Application.Reviews.Queries;

[McpServerToolType]
public static partial class ReviewTools
{
    /// <summary>
    /// List reviews with their star rating and the title and author of each, in movie-title order. Filter by movie, by
    /// author or both, or pass a reviewId to get exactly one review. With no filters it pages through every review.
    /// </summary>
    /// <param name="movieId">Only reviews of this movie. Use movies_search to find the id.</param>
    /// <param name="authorId">Only reviews by this author. Use authors_search to find the id.</param>
    /// <param name="reviewId">Get this one review instead of searching. The other filters are ignored.</param>
    /// <param name="offset">The number of results to skip, 0 for the first page. Pass the nextOffset from a previous call to get the next page.</param>
    /// <param name="limit">The most results to return, from 1 to 50.</param>
    [McpServerTool(Name = "reviews_list", Title = "List reviews", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true, IconSource = ItemIcons.Review)]
    public static async partial Task<ResultPage<ReviewResult>> ListReviews(
        ISender sender,
        CancellationToken cancellationToken,
        Guid? movieId = null,
        Guid? authorId = null,
        Guid? reviewId = null,
        int offset = 0,
        int limit = 10)
    {
        if (reviewId.HasValue)
        {
            var review = await sender.Send(new Queries.GetReviewById.GetReviewByIdQuery
            {
                Id = reviewId.Value
            }, cancellationToken);

            return new ResultPage<ReviewResult>([review.ToResult()], 1, null);
        }

        var page = await sender.Send(new Queries.GetReviews.GetReviewsQuery
        {
            MovieId = movieId,
            AuthorId = authorId,
            Offset = offset,
            Limit = limit
        }, cancellationToken);

        return page.ToResult(r => r.ToResult());
    }

    /// <summary>
    /// Create a review of a movie by an author with a star rating from 1 to 5, or pass a reviewId to change that review's
    /// stars. The movie and author of a review can't change. Returns the saved review. Without a reviewId every call
    /// creates another review, so don't retry a call that succeeded.
    /// </summary>
    /// <param name="stars">The star rating, a whole number from 1 (worst) to 5 (best).</param>
    /// <param name="movieId">The id of the movie being reviewed, needed to create a review. Use movies_search to find it.</param>
    /// <param name="authorId">The id of the author writing the review, needed to create a review. Use authors_search to find it.</param>
    /// <param name="reviewId">The id of an existing review to change the stars of. Leave empty to create a new review.</param>
    [McpServerTool(Name = "reviews_write", Title = "Write review", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false, UseStructuredContent = true, IconSource = ItemIcons.Review)]
    public static async partial Task<ReviewResult> WriteReview(
        ISender sender,
        int stars,
        CancellationToken cancellationToken,
        Guid? movieId = null,
        Guid? authorId = null,
        Guid? reviewId = null)
    {
        if (!reviewId.HasValue)
        {
            var created = await sender.Send(new Commands.CreateReview.CreateReviewCommand
            {
                AuthorId = authorId ?? Guid.Empty,
                MovieId = movieId ?? Guid.Empty,
                Stars = stars
            }, cancellationToken);

            return created.ToResult();
        }

        var updated = await sender.Send(new Commands.UpdateReview.UpdateReviewCommand
        {
            Id = reviewId.Value,
            Stars = stars
        }, cancellationToken);

        return updated.ToResult();
    }

    /// <summary>Permanently delete a review. This can't be undone, so confirm with the user first.</summary>
    /// <param name="reviewId">The id of the review to delete. Use reviews_list to find it.</param>
    [McpServerTool(Name = "reviews_delete", Title = "Delete review", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true, IconSource = ItemIcons.Review)]
    public static async partial Task<DeletedReview> DeleteReview(
        ISender sender,
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        // A review that isn't there throws not found, so getting here means it was deleted.
        _ = await sender.Send(new Commands.DeleteReview.DeleteReviewCommand
        {
            Id = reviewId
        }, cancellationToken);

        return new DeletedReview(reviewId, true);
    }
}
