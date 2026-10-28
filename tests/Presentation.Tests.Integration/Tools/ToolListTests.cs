namespace CleanModelContextProtocol.Presentation.Tests.Integration.Tools;

using Shouldly;
using Xunit;

public class ToolListTests : BaseToolTests
{
    public static TheoryData<string, string, bool, bool, bool> Tools => new()
    {
        { "authors_list", "List authors", true, false, true },
        { "authors_get", "Get author", true, false, true },
        { "movies_list", "List movies", true, false, true },
        { "movies_get", "Get movie", true, false, true },
        { "reviews_list", "List reviews", true, false, true },
        { "reviews_get", "Get review", true, false, true },
        { "reviews_create", "Create review", false, false, false },
        { "reviews_update", "Update review", false, true, true },
        { "reviews_delete", "Delete review", false, true, true },
        { "server_get_version", "Get server version", true, false, true }
    };

    [Fact]
    public async Task ListTools_ShouldReturn_EveryTool()
    {
        // Act
        var tools = await this.Client.ListToolsAsync();

        // Assert
        tools.Select(t => t.Name).ShouldBe(Tools.Select(t => (string)t[0]), ignoreOrder: true);
    }

    [Theory]
    [MemberData(nameof(Tools))]
    public async Task ListTools_ShouldDescribe_Tool(string name, string title, bool readOnly, bool destructive, bool idempotent)
    {
        // Act
        var tool = (await this.Client.ListToolsAsync()).Single(t => t.Name == name).ProtocolTool;

        // Assert
        tool.Title.ShouldBe(title);
        tool.Description.ShouldNotBeNullOrWhiteSpace();

        _ = tool.Annotations.ShouldNotBeNull();
        tool.Annotations.ReadOnlyHint.ShouldBe(readOnly);
        tool.Annotations.OpenWorldHint.ShouldBe(false);
        tool.Annotations.IdempotentHint.ShouldBe(idempotent);

        if (!readOnly)
        {
            tool.Annotations.DestructiveHint.ShouldBe(destructive);
        }

        foreach (var property in tool.InputSchema.GetProperty("properties").EnumerateObject())
        {
            property.Value.GetProperty("description").GetString().ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task ServerInfo_ShouldDescribe_Server()
    {
        // Assert
        var serverInfo = this.Client.ServerInfo;

        serverInfo.Name.ShouldBe("clean-model-context-protocol");
        serverInfo.Title.ShouldBe("Clean Model Context Protocol");
        this.Client.ServerInstructions.ShouldNotBeNullOrWhiteSpace();

        await Task.CompletedTask;
    }
}
