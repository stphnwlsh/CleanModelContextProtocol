namespace CleanModelContextProtocol.Presentation.Tools;

using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

/// <summary>
/// Adds a resource link for each movie, author or review a tool returns, so a client can open it as a resource. A tool
/// returns a typed result that the SDK turns into structured content, which leaves no room for a link there. The
/// resource's scheme is the start of the tool's name: movies_get links movies://{id}.
/// </summary>
public static class ResourceLinkFilter
{
    private const string MimeType = "application/json";

    public static McpRequestHandler<CallToolRequestParams, CallToolResult> Filter(McpRequestHandler<CallToolRequestParams, CallToolResult> next)
    {
        return async (request, cancellationToken) =>
        {
            var result = await next(request, cancellationToken);

            if (result.IsError != true && result.StructuredContent is { } content)
            {
                var scheme = request.Params!.Name.Split('_')[0];

                foreach (var link in GetItems(content).Select(i => ToLink(scheme, i)).OfType<ResourceLinkBlock>())
                {
                    result.Content.Add(link);
                }
            }

            return result;
        };
    }

    private static List<JsonElement> GetItems(JsonElement content)
    {
        return content.TryGetProperty("items", out var items) ? [.. items.EnumerateArray()] : [content];
    }

    private static ResourceLinkBlock? ToLink(string scheme, JsonElement item)
    {
        // A deleted review has no id, because there's nothing left to open.
        return GetText(item, "id") is { } id
            ? new ResourceLinkBlock { Uri = $"{scheme}://{id}", Name = GetName(item), MimeType = MimeType }
            : null;
    }

    private static string GetName(JsonElement item)
    {
        return GetText(item, "title")
            ?? GetText(item, "name")
            ?? $"Review of {GetText(item, "movieTitle")} by {GetText(item, "authorName")}";
    }

    private static string? GetText(JsonElement item, string name)
    {
        return item.TryGetProperty(name, out var value) ? value.GetString() : null;
    }
}
