namespace CleanModelContextProtocol.Presentation.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol;

/// <summary>
/// The JSON settings shared by tool results, resources and prompts.
/// </summary>
public static class McpSerializer
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(McpJsonUtilities.DefaultOptions)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        options.MakeReadOnly();

        return options;
    }
}
