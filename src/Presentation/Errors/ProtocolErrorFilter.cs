namespace CleanModelContextProtocol.Presentation.Errors;

using CleanModelContextProtocol.Application.Common.Exceptions;
using FluentValidation;
using ModelContextProtocol;
using ModelContextProtocol.Server;

/// <summary>
/// Maps exceptions raised by the Application layer while reading a resource or getting a prompt to MCP protocol errors.
/// </summary>
public static class ProtocolErrorFilter
{
    public static McpRequestHandler<TParams, TResult> Filter<TParams, TResult>(McpRequestHandler<TParams, TResult> next)
    {
        return async (request, cancellationToken) =>
        {
            try
            {
                return await next(request, cancellationToken);
            }
            catch (Exception exception) when (ToProtocolException(exception) is { } protocolException)
            {
                throw protocolException;
            }
        };
    }

    public static McpProtocolException? ToProtocolException(Exception exception)
    {
        return exception switch
        {
            NotFoundException => new McpProtocolException(
                $"Not Found: {exception.Message} Check the id: movies_search, authors_search and reviews_list return valid ones.",
                exception,
                McpErrorCode.ResourceNotFound),
            ValidationException validationException => new McpProtocolException(
                "Validation Failed: " + string.Join(" ", validationException.Errors
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                    .Distinct()),
                exception,
                McpErrorCode.InvalidParams),
            _ => null
        };
    }
}
