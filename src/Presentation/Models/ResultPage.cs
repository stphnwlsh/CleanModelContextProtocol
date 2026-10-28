namespace CleanModelContextProtocol.Presentation.Models;

using System.ComponentModel;

/// <summary>
/// One page of results from a search tool.
/// </summary>
/// <typeparam name="T">The type of the results.</typeparam>
/// <param name="Items">The results on this page.</param>
/// <param name="TotalCount">The number of results across every page.</param>
/// <param name="NextCursor">Pass this as the cursor to get the next page. Null when this is the last page.</param>
public record ResultPage<T>(
    [property: Description("The results on this page.")] List<T> Items,
    [property: Description("The number of results across every page.")] int TotalCount,
    [property: Description("Pass this as the cursor to get the next page. Null when this is the last page.")] string NextCursor);
