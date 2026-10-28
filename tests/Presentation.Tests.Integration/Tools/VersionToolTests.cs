namespace CleanModelContextProtocol.Presentation.Tests.Integration.Tools;

using Extensions;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;

public class VersionToolTests : BaseToolTests
{
    [Fact]
    public async Task GetVersion_ShouldReturn_Version()
    {
        // Act
        var result = await this.Client.CallToolAsync("server_get_version");
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        var version = text.Deserialize<Application.Versions.Entities.Version>();

        // Assert
        _ = version.ShouldNotBeNull();

        version.FileVersion.ShouldNotBeNullOrWhiteSpace();
        version.InformationalVersion.ShouldNotBeNullOrWhiteSpace();

        _ = result.StructuredContent.ShouldNotBeNull();
        result.StructuredContent.Value.GetProperty("fileVersion").GetString().ShouldBe(version.FileVersion);
    }
}
