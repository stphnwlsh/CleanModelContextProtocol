namespace CleanModelContextProtocol.Presentation.Tests.Integration;

using Extensions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;

public abstract class BaseToolTests : IAsyncLifetime, IAsyncDisposable
{
#pragma warning disable CS8618
    private readonly ModelContextProtocolApplication application;
    private bool disposed;

    protected McpClient Client { get; private set; }
#pragma warning restore CS8618

    protected async Task<T> CallAsync<T>(string name, Dictionary<string, object?>? arguments = null)
    {
        var result = await this.Client.CallToolAsync(name, arguments);

        result.IsError.ShouldNotBe(true, result.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text);

        return result.Content.OfType<TextContentBlock>().First().Text.Deserialize<T>();
    }

    protected async Task<string> CallForErrorAsync(string name, Dictionary<string, object?> arguments)
    {
        var result = await this.Client.CallToolAsync(name, arguments);

        result.IsError.ShouldBe(true);

        return result.Content.OfType<TextContentBlock>().First().Text;
    }

    public async Task InitializeAsync()
    {
        this.application = new ModelContextProtocolApplication();
        this.Client = await McpClient.CreateAsync(
            new HttpClientTransport(
                new HttpClientTransportOptions { Endpoint = new Uri("http://localhost/mcp") },
                this.application.CreateClient(),
                null,
                false),
            new McpClientOptions { ClientInfo = new Implementation { Name = "test-client", Version = "1.0" } });
    }

    public async Task DisposeAsync()
    {
        if (!this.disposed)
        {
            await this.Client.DisposeAsync();
            this.application.Dispose();
            this.disposed = true;
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await this.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
