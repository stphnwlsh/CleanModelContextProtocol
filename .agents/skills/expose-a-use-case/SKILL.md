---
name: expose-a-use-case
description: How to put an Application use case on the MCP server as a tool, resource or prompt - partial static methods with McpServer attributes, XML comments that become the descriptions, result models, completions and tests. Use when adding or changing a tool, resource or prompt on this MCP server.
metadata:
  scope: presentation
---

# Expose a use case (MCP)

## The rule

The server offers three kinds of thing, each in its own folder of `src/Presentation`, one `public static partial class` per feature:

| Kind | Folder | Attribute | Returns |
| --- | --- | --- | --- |
| Tool, something the model calls | `Tools/` | `[McpServerToolType]` on the class, `[McpServerTool(...)]` on the method | A result model from `Models/`, as structured content |
| Resource, something the client reads by URI | `Resources/` | `[McpServerResourceType]`, `[McpServerResource(UriTemplate = ...)]` | A JSON string from `McpSerializer.Serialize` |
| Prompt, a ready-made request the user picks | `Prompts/` | `[McpServerPromptType]`, `[McpServerPrompt(...)]` | A `GetPromptResult` |

Each method is `public static async partial` (or `public static partial` with no awaits). The MCP SDK's source generator turns the method's XML `<summary>` and `<param>` comments into the `[Description]` attributes the model reads, so the comments are the documentation. Methods take `ISender sender` and `CancellationToken cancellationToken`, build the Application request, send it and map the result. They contain no rules: validation and not-found come from Application, and the error filters turn them into MCP errors (`api-conventions`).

The SDK finds every class in the assembly (`WithToolsFromAssembly`, `WithResourcesFromAssembly`, `WithPromptsFromAssembly`), so a new class needs no registration.

## Steps

1. Add the use case first (`add-a-use-case`).
2. Pick the kind: a tool when the model should decide to call it, a resource when the client reads a known thing by id or a fixed list such as `movies://top-rated`, a prompt when a person starts a task.
3. Choose the name, title and annotations for a tool (`api-conventions`).
4. Write the method with `sender` and `cancellationToken` first, then required inputs, then optional ones with defaults.
5. Document it: `<summary>` saying what it does, what it returns and which tool to call first; a `<param>` for every input except `sender` and the token, with allowed values and where to find ids. Put the XML comment above the attributes.
6. Return a result model from `src/Presentation/Models/` (records with `[property: Description]`, because the output schema reads attributes), mapped in `ResultMapper`. Add new models to `AppJsonSerializerContext` (`json-serialization`).
7. Build a prompt's messages with `PromptContent`, and embed the movie or author it's about when you can find one. A prompt argument that names a movie or author gets suggestions from `Completions/CompletionHandler.cs`; add its name there. So does the id in a resource template, which suggests ids by their first characters or the title or name they belong to.
8. Update the server instructions in `WebApplicationBuilderExtensions` if the model needs to know the new thing exists (`api-conventions`).
9. Test it twice: a unit test calling the static method with a faked `ISender` (`tests/Presentation.Tests.Unit/Tools/`), and an integration test through the MCP client (`presentation-integration-tests`). Add a tool to the list in `ToolListTests`.

## Example

```csharp
// src/Presentation/Tools/MovieTools.cs
/// <summary>Get one movie with its review count, average stars and every review, including each author's name.</summary>
/// <param name="movieId">The movie's id (a GUID). Use movies_search to find it.</param>
[McpServerTool(Name = "movies_get", Title = "Get movie", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
public static async partial Task<MovieResult> GetMovie(
    ISender sender,
    Guid movieId,
    CancellationToken cancellationToken)
{
    var movie = await sender.Send(new Queries.GetMovieById.GetMovieByIdQuery
    {
        Id = movieId
    }, cancellationToken);

    return movie.ToResult(includeReviews: true);
}
```

## Don't

- Write `[Description]` attributes on tool, resource or prompt methods or their parameters. Write XML comments and keep the method `partial`.
- Document `sender` or the cancellation token. They aren't arguments the model sees.
- Return Application entities from a tool. Return a result model.
- Catch exceptions in a tool. The filters do it.
- Add a tool for every query. Six tools cover the jobs a model has; extend one before adding another (`api-conventions`).

## Check

``` bash
dotnet build                                   # MCP002 warns when a documented method isn't partial
dotnet test tests/Presentation.Tests.Unit
dotnet test tests/Presentation.Tests.Integration
```
