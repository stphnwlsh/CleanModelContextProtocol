---
name: presentation-integration-tests
description: How this MCP server is tested end to end - the ModelContextProtocolApplication host, BaseToolTests with a real MCP client over HTTP, calling tools, reading resources and getting prompts, asserting error results and protocol errors, and testing with authentication on. Use when writing or fixing a test in tests/Presentation.Tests.Integration.
metadata:
  scope: presentation
---

# Presentation integration tests (MCP)

## The rule

`tests/Presentation.Tests.Integration` talks to the whole server the way a real client does. `ModelContextProtocolApplication` (a `WebApplicationFactory<Program>`) starts the server with the `local` environment, its own shared-cache in-memory SQLite database and the sample data on. `BaseToolTests` connects an `McpClient` to it over the streamable HTTP transport before each test and disposes both after.

| Helper | Use it to |
| --- | --- |
| `this.Client` | Any MCP call: `ListToolsAsync`, `ReadResourceAsync`, `GetPromptAsync`, `CompleteAsync` |
| `CallAsync<T>(name, arguments)` | Call a tool, check it didn't return an error, and read its result as `T` |
| `CallForErrorAsync(name, arguments)` | Call a tool that should fail, check `IsError`, and return the message |
| `ModelContextProtocolApplication(environment, settings, configureServices)` | Start a server with other settings or services, such as authentication on |

Results are read with `Deserialize<T>()`, which uses the server's own `McpSerializer.Options` (`json-serialization`).

## Steps

1. Tool, resource and prompt tests inherit `BaseToolTests`. Put them in `Tools/`, `Resources/` or `Prompts/`.
2. Find the rows you need through the server (search, then pick one with the property you need), not by hard-coded ids. Sample data ids are random.
3. For each tool, test the success paths and each error result, asserting the message the model will read (`test-design`).
4. For resources and prompts, assert the `McpProtocolException.ErrorCode` (`ResourceNotFound`, `InvalidParams`) and its message.
5. A tool's name, title and annotations are checked in `ToolListTests`; add a row for a new tool.
6. In a solution with authentication, tests that need it create their own `ModelContextProtocolApplication` with `Authentication:*` settings and a test signing key, and use a plain `HttpClient` or a client with a bearer token. See `Authentication/AuthenticationTests.cs`.

## Example

```csharp
// tests/Presentation.Tests.Integration/Tools/BaseToolTests.cs
protected async Task<T> CallAsync<T>(string name, Dictionary<string, object?>? arguments = null)
{
    var result = await this.Client.CallToolAsync(name, arguments);

    result.IsError.ShouldNotBe(true, result.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text);

    return result.Content.OfType<TextContentBlock>().First().Text.Deserialize<T>();
}
```

```csharp
// tests/Presentation.Tests.Integration/Resources/ResourceTests.cs
[Fact]
public async Task ReadMovie_WithUnknownId_ShouldThrow_ResourceNotFound()
{
    // Act
    var exception = await Should.ThrowAsync<McpProtocolException>(() => this.Client.ReadResourceAsync($"movies://{Guid.NewGuid()}").AsTask());

    // Assert
    exception.ErrorCode.ShouldBe(McpErrorCode.ResourceNotFound);
    exception.Message.ShouldContain("The Movie with the supplied id was not found.");
}
```

## Don't

- Share one server across test classes. Each test gets its own server and database.
- Call tool methods directly in an integration test. That's what the unit tests do.
- Build your own `JsonSerializerOptions` in a test.
- Edit appsettings files for a test. Pass `settings`.
- Assume a result order the tool doesn't promise.

## Check

``` bash
dotnet test tests/Presentation.Tests.Integration
```
