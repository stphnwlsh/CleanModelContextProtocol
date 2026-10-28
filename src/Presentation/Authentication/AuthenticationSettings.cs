namespace CleanModelContextProtocol.Presentation.Authentication;

using Microsoft.Extensions.Configuration;

/// <summary>
/// OAuth settings for protecting the MCP endpoint. Leave the authority empty to run the server without authentication.
/// </summary>
public class AuthenticationSettings
{
    public const string SectionName = "Authentication";

    /// <summary>The OAuth authorization server that issues access tokens, such as https://login.example.com.</summary>
    public string Authority { get; init; }

    /// <summary>The audience access tokens must be issued for. This is usually the server's own URL.</summary>
    public string Audience { get; init; }

    /// <summary>The scopes clients should ask for, published in the protected resource metadata.</summary>
    public List<string> Scopes { get; init; } = [];

    public bool Enabled => !string.IsNullOrWhiteSpace(this.Authority);

    public static AuthenticationSettings FromConfiguration(IConfiguration configuration)
    {
        return configuration.GetSection(SectionName).Get<AuthenticationSettings>() ?? new AuthenticationSettings();
    }
}
