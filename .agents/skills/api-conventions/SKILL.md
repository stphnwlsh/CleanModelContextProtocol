---
name: api-conventions
description: How this MCP server's tools, resources and prompts are named and shaped - snake_case noun_verb tool names, titles and annotations, structured results with nextOffset paging, error results versus protocol errors, and keeping the server instructions current. Use when naming or changing a tool, resource or prompt, deciding what a tool returns, or changing how errors reach the model.
metadata:
  scope: presentation
---

# API conventions (MCP)

## The rule

The server is designed for a model to use well, not to mirror the Application layer. Six tools cover the jobs: `movies_search`, `movies_get`, `authors_search`, `reviews_list`, `reviews_write` and `reviews_delete`.

| Thing | Convention |
| --- | --- |
| Tool name | `<plural noun>_<verb>` in snake_case: `reviews_write` |
| Title | Sentence case for people: "Write review" |
| Annotations | Every tool sets `ReadOnly`, `Idempotent` and `OpenWorld = false`; writes also set `Destructive` |
| Results | `UseStructuredContent = true`, returning a record from `Models/`; lists return `ResultPage<T>` with `Items`, `TotalCount` and `NextOffset` (null on the last page) |
| Paging | `offset` and `limit` arguments, default 10, at most 50, the shared `paging` rules |
| Resource URIs | `<plural noun>://{id}` for a thing, `server://version` for the server; JSON only |
| Prompt names | snake_case verbs: `summarise_movie_reception` |
| Descriptions | Say what it returns, when to use it, and which tool finds the ids it needs |

Errors reach the model in two ways, both from `src/Presentation/Errors/`:

| Where | Filter | Shape |
| --- | --- | --- |
| Tools | `ToolErrorFilter` | A normal result with `IsError = true` and a message the model can act on: `Validation Failed: ...` or `Not Found: ...`, ending with what to do next |
| Resources and prompts | `ProtocolErrorFilter` | An `McpProtocolException`: `ResourceNotFound` for not found, `InvalidParams` for validation |

Anything else is unexpected. `ToolErrorFilter` logs it as `Error` and returns `Error: An error occurred while processing your request.`, because the model only sees a tool error and nothing else would log it. `ProtocolErrorFilter` lets it through, and the MCP SDK logs it and sends an internal error (`serilog`).

`ServerInstructions` in `WebApplicationBuilderExtensions` is the model's first read. It names the tools, how they connect and the rules (paging with `nextOffset`, confirm before deleting).

## Steps

1. Before adding a tool, check whether an argument on an existing one does the job (`reviews_list` takes `reviewId`, `reviews_write` creates or updates).
2. Set every annotation honestly. A tool that can overwrite data is `Destructive`; one that's safe to repeat is `Idempotent`.
3. Return the smallest useful result. Expensive detail is opt-in (`includeReviews`).
4. Write descriptions that steer: name the tool to call first, and warn when a call can't be undone or shouldn't be retried.
5. When a tool, resource or prompt is added, renamed or changes behaviour, update `ServerInstructions` in the same change.
6. Test the annotations in `ToolListTests` and each error message in the tool's integration tests (`test-design`).

## Resource links

Every tool that returns a movie, author or review also returns a `resource_link` block for it, as well as its structured content. The link's URI is the matching resource template, its name is the item's title or name, and its mime type is `application/json`. `ResourceLinkFilter` adds them, because a tool that returns a typed result has no way to add a content block itself. It takes the scheme from the start of the tool's name, so a new tool only has to follow the `movies_get` naming and return an `id`.

## Icons

Every tool, prompt and resource sets `IconSource` to a constant in `ItemIcons`, which points at a pinned brand release like the server's icon. Use the icon for what the item is about, not what it does. A new item needs one, or `EveryItem_ShouldHave_AnIcon` fails.

## Example

```csharp
// src/Presentation/Tools/ReviewTools.cs
[McpServerTool(Name = "reviews_delete", Title = "Delete review", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
public static async partial Task<DeletedReview> DeleteReview(
```

```csharp
// src/Presentation/Errors/ToolErrorFilter.cs
NotFoundException => $"Not Found: {exception.Message} Check the ids: movies_search, authors_search and reviews_list return valid ones.",
```

## Don't

- Throw from a tool to report a problem the model can fix. Let the filter return an error result.
- Return an unbounded list.
- Build a `CallToolResult` by hand in a tool to add a link. `ResourceLinkFilter` does that for every tool.
- Name tools after Application classes (`GetMovieByIdQuery`) or use camelCase or dashes.
- Leave `ServerInstructions` describing tools that changed.
- Put internal detail (stack traces, SQL) in an error message.

## Check

``` bash
dotnet test --filter "FullyQualifiedName~ToolListTests|FullyQualifiedName~Errors"
docker compose up -d --build   # then try it in the MCP Inspector (mcp-inspector)
```
