namespace CleanModelContextProtocol.Presentation.Tests.Integration;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

public abstract class BaseToolTests : IAsyncLifetime, IAsyncDisposable
{
#pragma warning disable CS8618
    private CleanModelContextProtocolApplication application;
    private bool disposed;

    protected McpClient Client { get; private set; }
#pragma warning restore CS8618

    public async Task InitializeAsync()
    {
        this.application = new CleanModelContextProtocolApplication();
        this.Client = await McpClient.CreateAsync(
            new HttpClientTransport(
                new HttpClientTransportOptions { Endpoint = new Uri("http://localhost") },
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
