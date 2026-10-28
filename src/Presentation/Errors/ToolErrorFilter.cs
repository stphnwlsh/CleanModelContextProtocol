namespace CleanModelContextProtocol.Presentation.Errors;

using CleanModelContextProtocol.Application.Common.Exceptions;
using FluentValidation;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

/// <summary>
/// Maps exceptions raised by the Application layer to MCP tool error results.
/// </summary>
public static class ToolErrorFilter
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
                return ToResult(exception, request?.Params?.Name);
            }
        };
    }

    public static CallToolResult ToResult(Exception exception, string toolName = null)
    {
        var message = exception switch
        {
            ValidationException validationException => "Validation Failed: " + string.Join(" ", validationException.Errors
                .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                .Distinct()) + " Correct the arguments and call the tool again.",
            NotFoundException => $"Not Found: {exception.Message}{ListToolHint(toolName)}",
            _ => "Error: An error occurred while processing your request."
        };

        return new CallToolResult
        {
            IsError = true,
            Content = [new TextContentBlock { Text = message }]
        };
    }

    private static string ListToolHint(string toolName)
    {
        if (toolName is null)
        {
            return string.Empty;
        }

        var separator = toolName.IndexOf('_');

        return separator > 0
            ? $" Check the ids, using {toolName[..separator]}_list or the other list tools to find valid ones."
            : string.Empty;
    }
}
