namespace CleanModelContextProtocol.Presentation.Completions;

using Application.Common.Paging;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using AuthorQueries = Application.Authors.Queries;
using MovieQueries = Application.Movies.Queries;
using ReviewQueries = Application.Reviews.Queries;

/// <summary>
/// Suggests movie titles and author names as the user types a prompt argument, and ids as they type a resource
/// template's id. An id can be found by its first characters, or by the title or name of what it belongs to.
/// </summary>
public static class CompletionHandler
{
    private const int MaxQueryLength = 100;

    public static async ValueTask<CompleteResult> Handle(RequestContext<CompleteRequestParams> context, CancellationToken cancellationToken)
    {
        return await Complete(context.Services!.GetRequiredService<ISender>(), context.Params, cancellationToken);
    }

    public static async Task<CompleteResult> Complete(ISender sender, CompleteRequestParams? request, CancellationToken cancellationToken)
    {
        var argument = request?.Argument;

        if (argument is null || argument.Value?.Length > MaxQueryLength)
        {
            return Empty();
        }

        return request?.Ref switch
        {
            PromptReference => await CompletePromptArgument(sender, argument, cancellationToken),
            ResourceTemplateReference => await CompleteResourceId(sender, argument, cancellationToken),
            _ => Empty()
        };
    }

    private static async Task<CompleteResult> CompletePromptArgument(ISender sender, Argument argument, CancellationToken cancellationToken)
    {
        switch (argument.Name)
        {
            case "movieTitle" or "firstMovieTitle" or "secondMovieTitle":
            {
                var page = await sender.Send(new MovieQueries.GetMovies.GetMoviesQuery
                {
                    Query = argument.Value,
                    Limit = Paging.MaxLimit
                }, cancellationToken);

                return Result([.. page.Items.Select(m => m.Title)], page.TotalCount);
            }

            case "authorName":
            {
                var page = await sender.Send(new AuthorQueries.GetAuthors.GetAuthorsQuery
                {
                    Query = argument.Value,
                    Limit = Paging.MaxLimit
                }, cancellationToken);

                return Result([.. page.Items.Select(a => $"{a.FirstName} {a.LastName}".Trim())], page.TotalCount);
            }

            default:
                return Empty();
        }
    }

    private static async Task<CompleteResult> CompleteResourceId(ISender sender, Argument argument, CancellationToken cancellationToken)
    {
        switch (argument.Name)
        {
            case "movieId":
            {
                var page = await sender.Send(new MovieQueries.GetMovies.GetMoviesQuery
                {
                    Query = argument.Value,
                    Limit = Paging.MaxLimit
                }, cancellationToken);

                if (page.Items.Count > 0)
                {
                    return Result([.. page.Items.Select(m => m.Id.ToString())], page.TotalCount);
                }

                var movies = await sender.Send(new MovieQueries.GetMovies.GetMoviesQuery { Limit = Paging.MaxLimit }, cancellationToken);

                return IdsStartingWith(movies.Items.Select(m => m.Id), argument.Value);
            }

            case "authorId":
            {
                var page = await sender.Send(new AuthorQueries.GetAuthors.GetAuthorsQuery
                {
                    Query = argument.Value,
                    Limit = Paging.MaxLimit
                }, cancellationToken);

                if (page.Items.Count > 0)
                {
                    return Result([.. page.Items.Select(a => a.Id.ToString())], page.TotalCount);
                }

                var authors = await sender.Send(new AuthorQueries.GetAuthors.GetAuthorsQuery { Limit = Paging.MaxLimit }, cancellationToken);

                return IdsStartingWith(authors.Items.Select(a => a.Id), argument.Value);
            }

            case "reviewId":
            {
                var reviews = await sender.Send(new ReviewQueries.GetReviews.GetReviewsQuery { Limit = Paging.MaxLimit }, cancellationToken);
                var value = argument.Value ?? string.Empty;
                var matches = reviews.Items
                    .Where(r => r.Id.ToString().StartsWith(value, StringComparison.OrdinalIgnoreCase)
                        || (r.ReviewedMovie?.Title.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false))
                    .Select(r => r.Id.ToString())
                    .ToList();

                return Result(matches, matches.Count);
            }

            default:
                return Empty();
        }
    }

    private static CompleteResult IdsStartingWith(IEnumerable<Guid> ids, string? value)
    {
        var matches = ids
            .Select(i => i.ToString())
            .Where(i => i.StartsWith(value ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Result(matches, matches.Count);
    }

    private static CompleteResult Result(List<string> values, int total)
    {
        return new CompleteResult
        {
            Completion = new Completion
            {
                Values = values,
                Total = total,
                HasMore = total > values.Count
            }
        };
    }

    private static CompleteResult Empty()
    {
        return Result([], 0);
    }
}
