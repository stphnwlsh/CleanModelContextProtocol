namespace CleanModelContextProtocol.Presentation.Tests.Integration.Authentication;

using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;

public class AuthenticationTests : IDisposable
{
    private const string Authority = "https://login.example.com";
    private const string Audience = "https://mcp.example.com";

    private static readonly SymmetricSecurityKey SigningKey = new(Encoding.UTF8.GetBytes("a-test-signing-key-that-is-long-enough-for-hs256"));

    private readonly ModelContextProtocolApplication application = new(
        settings: new Dictionary<string, string>
        {
            ["Authentication:Authority"] = Authority,
            ["Authentication:Audience"] = Audience,
            ["Authentication:Scopes:0"] = "mcp:tools"
        },
        configureServices: services => services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Configuration = new OpenIdConnectConfiguration { Issuer = Authority };
            options.TokenValidationParameters.IssuerSigningKey = SigningKey;
            options.TokenValidationParameters.ValidIssuer = Authority;
        }));

    [Fact]
    public async Task Mcp_WithoutToken_ShouldReturn_UnauthorizedWithResourceMetadata()
    {
        // Arrange
        using var client = this.application.CreateClient();
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");

        // Act
        using var response = await client.PostAsync(new Uri("http://localhost/mcp"), content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.WwwAuthenticate.ToString().ShouldContain("resource_metadata=\"http://localhost/.well-known/oauth-protected-resource/mcp\"");
    }

    [Fact]
    public async Task ResourceMetadata_ShouldReturn_AuthorizationServer()
    {
        // Arrange
        using var client = this.application.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("http://localhost/.well-known/oauth-protected-resource/mcp"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var metadata = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        metadata.RootElement.GetProperty("resource").GetString().ShouldBe(Audience);
        metadata.RootElement.GetProperty("authorization_servers")[0].GetString().ShouldBe(Authority);
        metadata.RootElement.GetProperty("scopes_supported")[0].GetString().ShouldBe("mcp:tools");
    }

    [Fact]
    public async Task Mcp_WithWrongAudience_ShouldReturn_Unauthorized()
    {
        // Arrange
        using var client = this.application.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken("https://someone-else.example.com"));
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");

        // Act
        using var response = await client.PostAsync(new Uri("http://localhost/mcp"), content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Mcp_WithToken_ShouldList_Tools()
    {
        // Arrange
        using var httpClient = this.application.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(Audience));

        await using var client = await McpClient.CreateAsync(
            new HttpClientTransport(
                new HttpClientTransportOptions { Endpoint = new Uri("http://localhost/mcp") },
                httpClient,
                null,
                false),
            new McpClientOptions { ClientInfo = new Implementation { Name = "test-client", Version = "1.0" } });

        // Act
        var tools = await client.ListToolsAsync();

        // Assert
        tools.ShouldNotBeEmpty();
    }

    public void Dispose()
    {
        this.application.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string CreateToken(string audience)
    {
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Authority,
            Audience = audience,
            Subject = new ClaimsIdentity([new Claim("sub", "test-user")]),
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256)
        });
    }
}
