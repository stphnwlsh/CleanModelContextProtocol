namespace CleanModelContextProtocol.Presentation.Errors;

using CleanModelContextProtocol.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

/// <summary>
/// Maps exceptions raised by the Application layer to MCP tool error results. Anything it doesn't expect is logged
/// here as an error, because the client only sees a tool error and nothing else logs it.
/// </summary>
public static partial class ToolErrorFilter
{
    public static McpRequestHandler<CallToolRequestParams, CallToolResult> Filter(McpRequestHandler<CallToolRequestParams, CallToolResult> next)
    {
        return async (request, cancellationToken) =>
        {
            try
            {
                return await next(request, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                if (exception is not (ValidationException or NotFoundException))
                {
                    var logger = request.Services?.GetService<ILoggerFactory>()?.CreateLogger(typeof(ToolErrorFilter)) ?? NullLogger.Instance;

                    LogUnexpected(logger, exception, request.Params?.Name);
                }

                return ToResult(exception);
            }
        };
    }

    public static CallToolResult ToResult(Exception exception)
    {
        var message = exception switch
        {
            ValidationException validationException => "Validation Failed: " + string.Join(" ", validationException.Errors
                .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                .Distinct()) + " Correct the arguments and call the tool again.",
            NotFoundException => $"Not Found: {exception.Message} Check the ids: movies_search, authors_search and reviews_list return valid ones.",
            _ => "Error: An error occurred while processing your request."
        };

        return new CallToolResult
        {
            IsError = true,
            Content = [new TextContentBlock { Text = message }]
        };
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "An unexpected exception stopped the {ToolName} tool")]
    private static partial void LogUnexpected(ILogger logger, Exception exception, string? toolName);
}
