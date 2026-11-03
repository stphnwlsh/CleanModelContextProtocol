namespace CleanModelContextProtocol.Presentation.Serialization;

using System.Text.Json.Serialization;
using Application.Versions.Entities;
using CleanModelContextProtocol.Presentation.Models;

/// <summary>
/// Generated JSON code for every type the tools, resources and prompts return, so they're written without reflection.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    WriteIndented = false,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(Version))]
[JsonSerializable(typeof(DeletedReview))]
[JsonSerializable(typeof(MovieResult))]
[JsonSerializable(typeof(AuthorResult))]
[JsonSerializable(typeof(ReviewResult))]
[JsonSerializable(typeof(ResultPage<MovieResult>))]
[JsonSerializable(typeof(ResultPage<AuthorResult>))]
[JsonSerializable(typeof(ResultPage<ReviewResult>))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
