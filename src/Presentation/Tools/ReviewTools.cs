namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using System.Text.Json;
using Mediator;
using ModelContextProtocol.Server;
using Commands = Application.Reviews.Commands;
using Queries = Application.Reviews.Queries;

[McpServerToolType]
public static class ReviewTools
{
    [McpServerTool(Name = "reviews_list", Title = "List reviews", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("List every review with its id, star rating (1 to 5), the movie it is for and the author who wrote it.")]
    public static async Task<string> ListReviews(ISender sender)
    {
        var result = await sender.Send(new Queries.GetReviews.GetReviewsQuery());

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "reviews_get", Title = "Get review", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Get one review by id, with its star rating, the movie it is for and the author who wrote it.")]
    public static async Task<string> GetReview(
        ISender sender,
        [Description("The review's id (a GUID). Use reviews_list to find it.")] Guid reviewId)
    {
        return JsonSerializer.Serialize(await sender.Send(new Queries.GetReviewById.GetReviewByIdQuery
        {
            Id = reviewId
        }));
    }

    [McpServerTool(Name = "reviews_create", Title = "Create review", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description("Create a review of a movie by an author with a star rating from 1 to 5, and return the new review. " +
        "Each call creates another review, so don't retry a call that succeeded.")]
    public static async Task<string> CreateReview(
        ISender sender,
        [Description("The id of the author writing the review. Use authors_list to find it.")] Guid authorId,
        [Description("The id of the movie being reviewed. Use movies_list to find it.")] Guid movieId,
        [Description("The star rating, a whole number from 1 (worst) to 5 (best).")] int stars)
    {
        var result = await sender.Send(new Commands.CreateReview.CreateReviewCommand
        {
            AuthorId = authorId,
            MovieId = movieId,
            Stars = stars
        });

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "reviews_update", Title = "Update review", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description("Replace an existing review's author, movie and star rating. Every field is overwritten, " +
        "so pass the current author and movie ids when you only want to change the stars.")]
    public static async Task<string> UpdateReview(
        ISender sender,
        [Description("The id of the review to update. Use reviews_list to find it.")] Guid reviewId,
        [Description("The id of the author writing the review. Use authors_list to find it.")] Guid authorId,
        [Description("The id of the movie being reviewed. Use movies_list to find it.")] Guid movieId,
        [Description("The star rating, a whole number from 1 (worst) to 5 (best).")] int stars)
    {
        _ = await sender.Send(new Commands.UpdateReview.UpdateReviewCommand
        {
            Id = reviewId,
            AuthorId = authorId,
            MovieId = movieId,
            Stars = stars
        });

        return "Review updated successfully";
    }

    [McpServerTool(Name = "reviews_delete", Title = "Delete review", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description("Permanently delete a review. This can't be undone, so confirm with the user first.")]
    public static async Task<string> DeleteReview(
        ISender sender,
        [Description("The id of the review to delete. Use reviews_list to find it.")] Guid reviewId)
    {
        _ = await sender.Send(new Commands.DeleteReview.DeleteReviewCommand
        {
            Id = reviewId
        });

        return "Review deleted successfully";
    }
}
