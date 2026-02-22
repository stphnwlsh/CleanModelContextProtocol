namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using System.Text.Json;
using CleanModelContextProtocol.Application.Common.Exceptions;
using MediatR;
using ModelContextProtocol.Server;
using Commands = Application.Reviews.Commands;
using Queries = Application.Reviews.Queries;

[McpServerToolType]
public static class ReviewTools
{
    [McpServerTool(Name = "GetAllReviews", ReadOnly = true)]
    [Description("Get all reviews in the system")]
    public static async Task<string> GetAllReviews(ISender sender)
    {
        try
        {
            var result = await sender.Send(new Queries.GetReviews.GetReviewsQuery());

            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "GetReviewById", ReadOnly = true)]
    [DisplayName("GetReviewById2")]
    [Description("Get a specific review by its unique ID")]
    public static async Task<string> GetReviewById(ISender sender, [Description("The GUID of the review")] Guid id)
    {
        try
        {
            return JsonSerializer.Serialize(await sender.Send(new Queries.GetReviewById.GetReviewByIdQuery
            {
                Id = id
            }));
        }
        catch (NotFoundException ex)
        {
            return $"Not Found: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "CreateReview")]
    [Description("Create a new review linking an author and a movie")]
    public static async Task<string> CreateReview(
        ISender sender,
        [Description("The GUID of the author writing the review")] Guid authorId,
        [Description("The GUID of the movie being reviewed")] Guid movieId,
        [Description("Star rating for the movie (e.g. 1-5)")] int stars)
    {
        try
        {
            var result = await sender.Send(new Commands.CreateReview.CreateReviewCommand
            {
                AuthorId = authorId,
                MovieId = movieId,
                Stars = stars
            });

            return JsonSerializer.Serialize(result);
        }
        catch (NotFoundException ex)
        {
            return $"Not Found: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "UpdateReview")]
    [Description("Update an existing review")]
    public static async Task<string> UpdateReview(
        ISender sender,
        [Description("The GUID of the review to update")] Guid id,
        [Description("The GUID of the author writing the review")] Guid authorId,
        [Description("The GUID of the movie being reviewed")] Guid movieId,
        [Description("Star rating for the movie (e.g. 1-5)")] int stars)
    {
        try
        {
            _ = await sender.Send(new Commands.UpdateReview.UpdateReviewCommand
            {
                Id = id,
                AuthorId = authorId,
                MovieId = movieId,
                Stars = stars
            });

            return "Review updated successfully";
        }
        catch (NotFoundException ex)
        {
            return $"Not Found: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "DeleteReview")]
    [Description("Delete a review by its unique ID")]
    public static async Task<string> DeleteReview(ISender sender, [Description("The GUID of the review to delete")] Guid id)
    {
        try
        {
            _ = await sender.Send(new Commands.DeleteReview.DeleteReviewCommand
            {
                Id = id
            });

            return "Review deleted successfully";
        }
        catch (NotFoundException ex)
        {
            return $"Not Found: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
