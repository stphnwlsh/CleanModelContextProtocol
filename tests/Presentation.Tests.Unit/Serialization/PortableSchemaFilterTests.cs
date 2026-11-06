namespace CleanModelContextProtocol.Presentation.Tests.Unit.Serialization;

using System.Text.Json;
using ModelContextProtocol.Protocol;
using Presentation.Serialization;
using Shouldly;
using Xunit;

public class PortableSchemaFilterTests
{
    [Fact]
    public void ToPortable_ShouldSplit_NullableString()
    {
        // Arrange
        var schema = Parse(/*lang=json,strict*/ """{"description":"An id.","type":["string","null"],"format":"uuid","default":null}""");

        // Act
        var result = PortableSchemaFilter.ToPortable(schema);

        // Assert
        result.GetProperty("description").GetString().ShouldBe("An id.");
        result.GetProperty("default").ValueKind.ShouldBe(JsonValueKind.Null);
        result.TryGetProperty("type", out _).ShouldBeFalse();
        result.TryGetProperty("format", out _).ShouldBeFalse();

        var anyOf = result.GetProperty("anyOf");

        anyOf[0].GetProperty("type").GetString().ShouldBe("string");
        anyOf[0].GetProperty("format").GetString().ShouldBe("uuid");
        anyOf[1].GetProperty("type").GetString().ShouldBe("null");
    }

    [Fact]
    public void ToPortable_ShouldSplit_NullableNumber()
    {
        // Arrange
        var schema = Parse(/*lang=json,strict*/ """{"type":["number","null"]}""");

        // Act
        var result = PortableSchemaFilter.ToPortable(schema);

        // Assert
        result.GetProperty("anyOf")[0].GetProperty("type").GetString().ShouldBe("number");
    }

    [Fact]
    public void ToPortable_ShouldSplit_NullableArray_AndItsItems()
    {
        // Arrange
        var schema = Parse(/*lang=json,strict*/ """
            {"type":"object","properties":{"reviews":{"type":["array","null"],"items":{"type":"object","properties":{"title":{"type":["string","null"]}}}}}}
            """);

        // Act
        var result = PortableSchemaFilter.ToPortable(schema);

        // Assert
        var branch = result.GetProperty("properties").GetProperty("reviews").GetProperty("anyOf")[0];

        branch.GetProperty("type").GetString().ShouldBe("array");

        var title = branch.GetProperty("items").GetProperty("properties").GetProperty("title");

        title.GetProperty("anyOf")[0].GetProperty("type").GetString().ShouldBe("string");
    }

    [Fact]
    public void ToPortable_ShouldLeave_NonNullableFieldUnchanged()
    {
        // Arrange
        var schema = Parse(/*lang=json,strict*/ """{"type":"object","properties":{"stars":{"description":"Stars.","type":"integer"}},"required":["stars"]}""");

        // Act
        var result = PortableSchemaFilter.ToPortable(schema);

        // Assert
        result.GetRawText().ShouldBe(schema.GetRawText());
    }

    [Fact]
    public async Task Filter_ShouldRewrite_InputAndOutputSchemas()
    {
        // Arrange
        var nullable = Parse(/*lang=json,strict*/ """{"type":"object","properties":{"id":{"type":["string","null"]}}}""");
        var withOutput = new Tool { Name = "with_output", InputSchema = nullable, OutputSchema = nullable };
        var withoutOutput = new Tool { Name = "without_output", InputSchema = nullable };
        var handler = PortableSchemaFilter.Filter((request, cancellationToken) =>
            ValueTask.FromResult(new ListToolsResult { Tools = [withOutput, withoutOutput] }));

        // Act
        var result = await handler(null!, CancellationToken.None);

        // Assert
        result.Tools[0].InputSchema.GetRawText().ShouldNotContain("[\"string\",\"null\"]");
        result.Tools[0].OutputSchema.ShouldNotBeNull().GetRawText().ShouldNotContain("[\"string\",\"null\"]");
        result.Tools[1].OutputSchema.ShouldBeNull();
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);

        return document.RootElement.Clone();
    }
}
