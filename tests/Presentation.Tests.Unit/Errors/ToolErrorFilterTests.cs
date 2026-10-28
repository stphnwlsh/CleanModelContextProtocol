namespace CleanModelContextProtocol.Presentation.Tests.Unit.Errors;

using CleanModelContextProtocol.Application.Common.Exceptions;
using CleanModelContextProtocol.Presentation.Errors;
using FluentValidation;
using FluentValidation.Results;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;

public class ToolErrorFilterTests
{
    [Fact]
    public async Task Filter_ShouldReturn_Result()
    {
        // Arrange
        var expected = new CallToolResult { Content = [new TextContentBlock { Text = "Lorem Ipsum" }] };
        var handler = ToolErrorFilter.Filter((request, cancellationToken) => ValueTask.FromResult(expected));

        // Act
        var result = await handler(null!, CancellationToken.None);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Filter_ShouldReturn_ErrorResult()
    {
        // Arrange
        var handler = ToolErrorFilter.Filter((request, cancellationToken) => throw new NotFoundException("Expected Exception"));

        // Act
        var result = await handler(null!, CancellationToken.None);

        // Assert
        result.IsError.ShouldBe(true);
        result.Content.OfType<TextContentBlock>().Single().Text.ShouldBe("Not Found: Expected Exception");
    }

    [Fact]
    public void ToResult_ShouldReturn_ValidationError()
    {
        // Arrange
        var exception = new ValidationException(
        [
            new ValidationFailure("AuthorId", "An author id was not supplied to create the review."),
            new ValidationFailure("Stars", "A star rating must be between 1 and 5.")
        ]);

        // Act
        var result = ToolErrorFilter.ToResult(exception);

        // Assert
        result.IsError.ShouldBe(true);
        result.Content.OfType<TextContentBlock>().Single().Text
            .ShouldBe("Validation Failed: AuthorId: An author id was not supplied to create the review. Stars: A star rating must be between 1 and 5. Correct the arguments and call the tool again.");
    }

    [Fact]
    public void ToResult_ShouldReturn_NotFoundError()
    {
        // Act
        var result = ToolErrorFilter.ToResult(new NotFoundException("Expected Exception"));

        // Assert
        result.IsError.ShouldBe(true);
        result.Content.OfType<TextContentBlock>().Single().Text.ShouldBe("Not Found: Expected Exception");
    }

    [Theory]
    [InlineData("movies_get", "Not Found: Expected Exception Check the ids, using movies_list or the other list tools to find valid ones.")]
    [InlineData("server", "Not Found: Expected Exception")]
    [InlineData("_get", "Not Found: Expected Exception")]
    public void ToResult_ShouldReturn_NotFoundErrorWithHint(string toolName, string expected)
    {
        // Act
        var result = ToolErrorFilter.ToResult(new NotFoundException("Expected Exception"), toolName);

        // Assert
        result.IsError.ShouldBe(true);
        result.Content.OfType<TextContentBlock>().Single().Text.ShouldBe(expected);
    }

    [Fact]
    public void ToResult_ShouldReturn_Error()
    {
        // Act
        var result = ToolErrorFilter.ToResult(new ArgumentException("Expected Exception"));

        // Assert
        result.IsError.ShouldBe(true);
        result.Content.OfType<TextContentBlock>().Single().Text.ShouldBe("Error: An error occurred while processing your request.");
    }
}
