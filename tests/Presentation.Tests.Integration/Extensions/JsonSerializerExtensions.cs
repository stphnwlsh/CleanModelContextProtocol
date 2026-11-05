namespace CleanModelContextProtocol.Presentation.Tests.Integration.Extensions;

using System.Text.Json;
using Presentation.Serialization;

public static class JsonSerializerExtensions
{
    // The server's own settings, so a test reads a result exactly the way a client would.
    public static T Deserialize<T>(this string input)
    {
        return JsonSerializer.Deserialize<T>(input, McpSerializer.Options)!;
    }
}
