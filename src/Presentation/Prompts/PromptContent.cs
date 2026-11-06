namespace CleanModelContextProtocol.Presentation.Prompts;

using ModelContextProtocol.Protocol;

/// <summary>
/// The messages a prompt is made of: an instruction, and the movie or author it's about.
/// </summary>
internal static class PromptContent
{
    public static PromptMessage Text(string text)
    {
        return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock { Text = text }
        };
    }

    public static PromptMessage Embed(string uri, string json)
    {
        return new PromptMessage
        {
            Role = Role.User,
            Content = new EmbeddedResourceBlock
            {
                Resource = new TextResourceContents
                {
                    Uri = uri,
                    MimeType = "application/json",
                    Text = json
                }
            }
        };
    }
}
