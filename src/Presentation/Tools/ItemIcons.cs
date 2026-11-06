namespace CleanModelContextProtocol.Presentation.Tools;

/// <summary>
/// The icons on tools, prompts and resources, from a pinned brand release like the server's own icon. They point at
/// the brand repository, not this one, because dotnet new renames the template's name wherever it appears.
/// </summary>
public static class ItemIcons
{
    public const string Movie = Source + "movie-64.png";
    public const string Author = Source + "author-64.png";
    public const string Review = Source + "review-64.png";
    public const string Server = Source + "server-64.png";

    private const string Source = "https://raw.githubusercontent.com/LeanMeanCleanApps/brand/2026.11.06/assets/icons/mcp/png/";
}
