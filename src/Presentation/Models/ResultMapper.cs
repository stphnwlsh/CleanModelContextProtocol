namespace CleanModelContextProtocol.Presentation.Models;

using Application.Authors.Entities;
using Application.Common.Paging;
using Application.Movies.Entities;
using Application.Reviews.Entities;

/// <summary>
/// Maps Application entities onto the results the tools return.
/// </summary>
public static class ResultMapper
{
    public static MovieResult ToResult(this Movie movie, bool includeReviews)
    {
        var reviews = movie.Reviews;

        return new MovieResult(
            movie.Id,
            movie.Title,
            reviews.Count,
            Average(reviews),
            includeReviews ? [.. reviews.Select(r => r.ToResult(movie: new ReviewedMovie(movie.Id, movie.Title)))] : null);
    }

    public static MovieResult ToResult(this MovieSummary movie, IEnumerable<Review>? reviews = null)
    {
        return new MovieResult(
            movie.Id,
            movie.Title,
            movie.ReviewCount,
            movie.AverageStars,
            reviews is null ? null : [.. reviews.Select(r => r.ToResult(movie: new ReviewedMovie(movie.Id, movie.Title)))]);
    }

    public static AuthorResult ToResult(this Author author, bool includeReviews)
    {
        var reviews = author.Reviews;

        return new AuthorResult(
            author.Id,
            FullName(author.FirstName, author.LastName),
            reviews.Count,
            Average(reviews),
            includeReviews ? [.. reviews.Select(r => r.ToResult(author: new ReviewAuthor(author.Id, author.FirstName, author.LastName)))] : null);
    }

    public static AuthorResult ToResult(this AuthorSummary author, IEnumerable<Review>? reviews = null)
    {
        return new AuthorResult(
            author.Id,
            FullName(author.FirstName, author.LastName),
            author.ReviewCount,
            author.AverageStars,
            reviews is null ? null : [.. reviews.Select(r => r.ToResult(author: new ReviewAuthor(author.Id, author.FirstName, author.LastName)))]);
    }

    public static ReviewResult ToResult(this Review review, ReviewedMovie? movie = null, ReviewAuthor? author = null)
    {
        movie = review.ReviewedMovie ?? movie;
        author = review.ReviewAuthor ?? author;

        return new ReviewResult(
            review.Id,
            review.Stars,
            movie?.Id,
            movie?.Title,
            author?.Id,
            author is null ? null : FullName(author.FirstName, author.LastName));
    }

    public static ResultPage<TResult> ToResult<T, TResult>(this Page<T> page, Func<T, TResult> map)
    {
        var nextOffset = page.Offset + page.Items.Count;

        return new ResultPage<TResult>([.. page.Items.Select(map)], page.TotalCount, nextOffset < page.TotalCount ? nextOffset : null);
    }

    private static string FullName(string firstName, string lastName)
    {
        return $"{firstName} {lastName}".Trim();
    }

    private static double? Average(ICollection<Review> reviews)
    {
        return reviews.Count == 0 ? null : Math.Round(reviews.Average(r => r.Stars), 1, MidpointRounding.AwayFromZero);
    }
}
