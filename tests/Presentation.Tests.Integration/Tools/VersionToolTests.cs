namespace CleanModelContextProtocol.Presentation.Tests.Integration.Tools;

using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;

public class VersionToolTests : BaseToolTests
{
    [Fact]
    public async Task GetVersion_ShouldReturn_Version()
    {
        // Act
        var result = await this.Client.CallToolAsync("GetVersion");
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Assert
        _ = text.ShouldNotBeNull();
        _ = text.ShouldBeOfType<string>();

        text.ShouldNotBeNullOrWhiteSpace();
        text.ShouldContain("File Version");
        text.ShouldContain("Informational Version");
    }
}
