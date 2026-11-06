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

    // The most values the protocol allows in one completion.
    private const int MaxValues = 100;

    public static async ValueTask<CompleteResult> Handle(RequestContext<CompleteRequestParams> context, CancellationToken cancellationToken)
    {
        return await Complete(context.Services!.GetRequiredService<ISender>(), context.Params, cancellationToken);
    }

    public static async Task<CompleteResult> Complete(ISender sender, CompleteRequestParams? request, CancellationToken cancellationToken)
    {
        if (request?.Argument is not { } argument || argument.Value.Length > MaxQueryLength)
        {
            return Empty();
        }

        return request.Ref is PromptReference
            ? await CompletePromptArgument(sender, argument, cancellationToken)
            : await CompleteResourceId(sender, argument, cancellationToken);
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

            case "movieId":
                return await CompleteResourceId(sender, argument, cancellationToken);

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

                var movies = await GetAll(offset => sender.Send(new MovieQueries.GetMovies.GetMoviesQuery
                {
                    Offset = offset,
                    Limit = Paging.MaxLimit
                }, cancellationToken));

                return IdsStartingWith(movies.Select(m => m.Id), argument.Value);
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

                var authors = await GetAll(offset => sender.Send(new AuthorQueries.GetAuthors.GetAuthorsQuery
                {
                    Offset = offset,
                    Limit = Paging.MaxLimit
                }, cancellationToken));

                return IdsStartingWith(authors.Select(a => a.Id), argument.Value);
            }

            case "reviewId":
            {
                var reviews = await GetAll(offset => sender.Send(new ReviewQueries.GetReviews.GetReviewsQuery
                {
                    Offset = offset,
                    Limit = Paging.MaxLimit
                }, cancellationToken));
                var matches = reviews
                    .Where(r => r.Id.ToString().StartsWith(argument.Value, StringComparison.OrdinalIgnoreCase)
                        || (r.ReviewedMovie?.Title.Contains(argument.Value, StringComparison.OrdinalIgnoreCase) ?? false))
                    .Select(r => r.Id.ToString())
                    .ToList();

                return Capped(matches);
            }

            default:
                return Empty();
        }
    }

    private static CompleteResult IdsStartingWith(IEnumerable<Guid> ids, string value)
    {
        var matches = ids
            .Select(i => i.ToString())
            .Where(i => i.StartsWith(value, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Capped(matches);
    }

    private static async Task<List<T>> GetAll<T>(Func<int, ValueTask<Page<T>>> getPage)
    {
        var items = new List<T>();
        Page<T> page;

        do
        {
            page = await getPage(items.Count);
            items.AddRange(page.Items);
        }
        while (items.Count < page.TotalCount);

        return items;
    }

    private static CompleteResult Capped(List<string> matches)
    {
        return Result([.. matches.Take(MaxValues)], matches.Count);
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
