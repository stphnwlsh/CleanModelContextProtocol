namespace CleanModelContextProtocol.Presentation.Tests.Unit.Errors;

using CleanModelContextProtocol.Application.Common.Exceptions;
using CleanModelContextProtocol.Presentation.Errors;
using FluentValidation;
using FluentValidation.Results;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using Shouldly;
using Xunit;

public class ProtocolErrorFilterTests
{
    [Fact]
    public async Task Filter_ShouldReturn_Result()
    {
        // Arrange
        var expected = new ReadResourceResult();
        var handler = ProtocolErrorFilter.Filter<ReadResourceRequestParams, ReadResourceResult>((request, cancellationToken) => ValueTask.FromResult(expected));

        // Act
        var result = await handler(null!, CancellationToken.None);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Filter_ShouldThrow_ResourceNotFound()
    {
        // Arrange
        var handler = ProtocolErrorFilter.Filter<ReadResourceRequestParams, ReadResourceResult>((request, cancellationToken) => throw new NotFoundException("Expected Exception"));

        // Act
        var exception = await Should.ThrowAsync<McpProtocolException>(() => handler(null!, CancellationToken.None).AsTask());

        // Assert
        exception.ErrorCode.ShouldBe(McpErrorCode.ResourceNotFound);
        exception.Message.ShouldBe("Not Found: Expected Exception Check the id: movies_search, authors_search and reviews_list return valid ones.");
    }

    [Fact]
    public async Task Filter_ShouldThrow_InvalidParams()
    {
        // Arrange
        var handler = ProtocolErrorFilter.Filter<GetPromptRequestParams, GetPromptResult>((request, cancellationToken) =>
            throw new ValidationException([new ValidationFailure("Query", "Too long."), new ValidationFailure("Query", "Too long.")]));

        // Act
        var exception = await Should.ThrowAsync<McpProtocolException>(() => handler(null!, CancellationToken.None).AsTask());

        // Assert
        exception.ErrorCode.ShouldBe(McpErrorCode.InvalidParams);
        exception.Message.ShouldBe("Validation Failed: Query: Too long.");
    }

    [Fact]
    public async Task Filter_ShouldRethrow_OtherExceptions()
    {
        // Arrange
        var handler = ProtocolErrorFilter.Filter<ReadResourceRequestParams, ReadResourceResult>((request, cancellationToken) => throw new InvalidOperationException("Expected Exception"));

        // Act & Assert
        _ = await Should.ThrowAsync<InvalidOperationException>(() => handler(null!, CancellationToken.None).AsTask());
    }
}
