namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using CleanModelContextProtocol.Presentation.Models;
using Mediator;
using ModelContextProtocol.Server;
using Commands = Application.Reviews.Commands;
using Queries = Application.Reviews.Queries;

[McpServerToolType]
public static class ReviewTools
{
    [McpServerTool(Name = "reviews_list", Title = "List reviews", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("List reviews with their star rating and the title and author of each, in movie-title order. Filter by movie, " +
        "by author or both, or pass a reviewId to get exactly one review. With no filters it pages through every review.")]
    public static async Task<ResultPage<ReviewResult>> ListReviews(
        ISender sender,
        [Description("Only reviews of this movie. Use movies_search to find the id.")] Guid? movieId = null,
        [Description("Only reviews by this author. Use authors_search to find the id.")] Guid? authorId = null,
        [Description("Get this one review instead of searching. The other filters are ignored.")] Guid? reviewId = null,
        [Description("The most results to return, from 1 to 50.")] int limit = 10,
        [Description("The nextCursor from a previous call, to get the next page. Leave empty for the first page.")] string cursor = null)
    {
        if (reviewId.HasValue)
        {
            var review = await sender.Send(new Queries.GetReviewById.GetReviewByIdQuery
            {
                Id = reviewId.Value
            });

            return new ResultPage<ReviewResult>([review.ToResult()], 1, null);
        }

        var page = await sender.Send(new Queries.SearchReviews.SearchReviewsQuery
        {
            MovieId = movieId,
            AuthorId = authorId,
            Limit = limit,
            Cursor = cursor
        });

        return page.ToResult(r => r.ToResult());
    }

    [McpServerTool(Name = "reviews_write", Title = "Write review", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Create a review of a movie by an author with a star rating from 1 to 5, or pass a reviewId to replace that " +
        "review's movie, author and stars. Returns the saved review. Without a reviewId every call creates another review, " +
        "so don't retry a call that succeeded.")]
    public static async Task<ReviewResult> WriteReview(
        ISender sender,
        [Description("The id of the movie being reviewed. Use movies_search to find it.")] Guid movieId,
        [Description("The id of the author writing the review. Use authors_search to find it.")] Guid authorId,
        [Description("The star rating, a whole number from 1 (worst) to 5 (best).")] int stars,
        [Description("The id of an existing review to replace. Leave empty to create a new review.")] Guid? reviewId = null)
    {
        if (!reviewId.HasValue)
        {
            var created = await sender.Send(new Commands.CreateReview.CreateReviewCommand
            {
                AuthorId = authorId,
                MovieId = movieId,
                Stars = stars
            });

            return created.ToResult();
        }

        _ = await sender.Send(new Commands.UpdateReview.UpdateReviewCommand
        {
            Id = reviewId.Value,
            AuthorId = authorId,
            MovieId = movieId,
            Stars = stars
        });

        var updated = await sender.Send(new Queries.GetReviewById.GetReviewByIdQuery
        {
            Id = reviewId.Value
        });

        return updated.ToResult();
    }

    [McpServerTool(Name = "reviews_delete", Title = "Delete review", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Permanently delete a review. This can't be undone, so confirm with the user first.")]
    public static async Task<DeletedReview> DeleteReview(
        ISender sender,
        [Description("The id of the review to delete. Use reviews_list to find it.")] Guid reviewId)
    {
        var deleted = await sender.Send(new Commands.DeleteReview.DeleteReviewCommand
        {
            Id = reviewId
        });

        return new DeletedReview(reviewId, deleted);
    }
}
