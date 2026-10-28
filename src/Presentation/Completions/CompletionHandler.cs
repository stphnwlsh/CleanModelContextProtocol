namespace CleanModelContextProtocol.Presentation.Completions;

using Application.Common.Paging;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using AuthorQueries = Application.Authors.Queries;
using MovieQueries = Application.Movies.Queries;

/// <summary>
/// Suggests movie titles and author names as the user types a prompt argument.
/// </summary>
public static class CompletionHandler
{
    private const int MaxQueryLength = 100;

    public static async ValueTask<CompleteResult> Handle(RequestContext<CompleteRequestParams> context, CancellationToken cancellationToken)
    {
        return await Complete(context.Services.GetRequiredService<ISender>(), context.Params, cancellationToken);
    }

    public static async Task<CompleteResult> Complete(ISender sender, CompleteRequestParams request, CancellationToken cancellationToken)
    {
        var argument = request?.Argument;

        if (request?.Ref is not PromptReference || argument is null || argument.Value?.Length > MaxQueryLength)
        {
            return Empty();
        }

        switch (argument.Name)
        {
            case "movieTitle" or "firstMovieTitle" or "secondMovieTitle":
            {
                var page = await sender.Send(new MovieQueries.SearchMovies.SearchMoviesQuery
                {
                    Query = argument.Value,
                    Limit = Paging.MaxLimit
                }, cancellationToken);

                return Result([.. page.Items.Select(m => m.Title)], page.TotalCount);
            }

            case "authorName":
            {
                var page = await sender.Send(new AuthorQueries.SearchAuthors.SearchAuthorsQuery
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
