namespace CleanModelContextProtocol.Presentation.Serialization;

using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

/// <summary>
/// Rewrites nullable fields in tool schemas from <c>"type": ["string", "null"]</c> to an <c>anyOf</c> of single types.
/// Both are valid JSON Schema, but clients that map schemas onto a single-type dialect, such as Gemini's function
/// declarations, can reject the first form or drop its constraints.
/// </summary>
public static class PortableSchemaFilter
{
    private static readonly HashSet<string> OuterKeywords = ["default", "description", "title"];

    public static McpRequestHandler<ListToolsRequestParams, ListToolsResult> Filter(McpRequestHandler<ListToolsRequestParams, ListToolsResult> next)
    {
        return async (request, cancellationToken) =>
        {
            var result = await next(request, cancellationToken);

            foreach (var tool in result.Tools)
            {
                tool.InputSchema = ToPortable(tool.InputSchema);

                if (tool.OutputSchema is { } outputSchema)
                {
                    tool.OutputSchema = ToPortable(outputSchema);
                }
            }

            return result;
        };
    }

    public static JsonElement ToPortable(JsonElement schema)
    {
        var node = JsonNode.Parse(schema.GetRawText());

        Rewrite(node);

        using var document = JsonDocument.Parse(node?.ToJsonString() ?? "null");

        return document.RootElement.Clone();
    }

    private static void Rewrite(JsonNode? node)
    {
        if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                Rewrite(item);
            }
        }

        if (node is JsonObject schema)
        {
            foreach (var property in schema)
            {
                Rewrite(property.Value);
            }

            if (schema["type"] is JsonArray types && types.Count == 2 && types.Any(IsNull))
            {
                SplitNullable(schema, types.First(t => !IsNull(t))!.GetValue<string>());
            }
        }
    }

    private static void SplitNullable(JsonObject schema, string type)
    {
        var branch = new JsonObject { ["type"] = type };

        _ = schema.Remove("type");

        foreach (var name in schema.Select(p => p.Key).Where(k => !OuterKeywords.Contains(k)).ToList())
        {
            _ = schema.Remove(name, out var value);
            branch[name] = value;
        }

        schema["anyOf"] = new JsonArray(branch, new JsonObject { ["type"] = "null" });
    }

    private static bool IsNull(JsonNode? type)
    {
        return type?.GetValue<string>() == "null";
    }
}
