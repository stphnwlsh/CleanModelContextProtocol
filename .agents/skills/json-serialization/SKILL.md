---
name: json-serialization
description: How this MCP server reads tool arguments and writes results with System.Text.Json - McpSerializer.Options built on the SDK defaults, the source-generated AppJsonSerializerContext and which types it lists, and how tests use the same options. Use when adding a result model, changing how JSON looks, or when a value serialises differently than expected.
metadata:
  scope: presentation
---

# JSON serialization (MCP)

## The rule

Tool arguments, tool results, resources and prompt attachments all use one set of options: `McpSerializer.Options` in `src/Presentation/Serialization/McpSerializer.cs`. It starts from the SDK's `McpJsonUtilities.DefaultOptions`, then sets camelCase names and leaves nulls out, and puts `AppJsonSerializerContext` first in the resolver chain so the listed types use generated code. The options are read-only once built.

| Used by | How |
| --- | --- |
| Tools and prompts | `WithToolsFromAssembly(serializerOptions: McpSerializer.Options)` and `WithPromptsFromAssembly(...)` |
| Resources and embedded prompt resources | `McpSerializer.Serialize(value)` |
| Integration tests | `Deserialize<T>()` in `tests/Presentation.Tests.Integration/Extensions/JsonSerializerExtensions.cs` |

`AppJsonSerializerContext` lists exactly the types the server writes: the result models, their `ResultPage<T>` forms and `Version`.

Results are trees. A review inside a movie carries the movie's id and title, not the movie, so there's no cycle handling.

Tool schemas are generated from the C# types, and a nullable field comes out as `"type": ["string", "null"]`. That's valid JSON Schema, but clients that map schemas onto a single-type dialect, such as Gemini's function declarations, can reject it or drop its constraints. `PortableSchemaFilter` rewrites each one to an `anyOf` of single types when tools are listed, keeping `description`, `title` and `default` on the outer node. It runs for every tool, so a new tool needs nothing extra. `ListTools_ShouldWrite_PortableSchemas` fails if a type array gets through.

The filter is a workaround for those clients, and nothing else needs it. Delete it, with that test, once they accept type arrays. To check, take it out and confirm the Inspector shows no schema portability warnings on any tool.

## Steps

1. Add every new result model, and the `ResultPage<T>` you return, to `AppJsonSerializerContext` with `[JsonSerializable(typeof(...))]`.
2. Remove a type from the context when nothing returns it any more.
3. Change how JSON looks only in `McpSerializer.CreateOptions`. Keep the context's `[JsonSourceGenerationOptions]` matching it.
4. Serialise resources with `McpSerializer.Serialize`, never `JsonSerializer.Serialize` with default options.

## Example

```csharp
// src/Presentation/Serialization/McpSerializer.cs
var options = new JsonSerializerOptions(McpJsonUtilities.DefaultOptions)
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
options.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
options.MakeReadOnly();
```

## Don't

- Build your own `JsonSerializerOptions`, including in tests.
- Add `ReferenceHandler.IgnoreCycles` or `Preserve` to hide a cycle. Change the result shape.
- List Application entities in the context unless a resource returns one directly.
- Use Newtonsoft.Json.
- Hand-write a tool schema to get around a portability warning. Change `PortableSchemaFilter`.

## Check

``` bash
dotnet test tests/Presentation.Tests.Integration
```
