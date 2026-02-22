namespace CleanModelContextProtocol.Presentation.Tools;

using System.ComponentModel;
using System.Text.Json;
using CleanModelContextProtocol.Application.Common.Exceptions;
using MediatR;
using ModelContextProtocol.Server;
using Queries = Application.Authors.Queries;

[McpServerToolType]
public static class AuthorTools
{
    [McpServerTool(Name = "GetAllAuthors", ReadOnly = true)]
    [Description("Get all authors in the system")]
    public static async Task<string> GetAllAuthors(ISender sender)
    {
        try
        {
            var result = await sender.Send(new Queries.GetAuthors.GetAuthorsQuery());

            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "GetAuthorById", ReadOnly = true)]
    [Description("Get a specific author by their unique ID")]
    public static async Task<string> GetAuthorById(ISender sender, [Description("The GUID of the author")] Guid id)
    {
        try
        {
            return JsonSerializer.Serialize(await sender.Send(new Queries.GetAuthorById.GetAuthorByIdQuery
            {
                Id = id
            }));
        }
        catch (NotFoundException ex)
        {
            return $"Not Found: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
