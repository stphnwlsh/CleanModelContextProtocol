namespace CleanModelContextProtocol.Presentation.Tests.Unit.Errors;

using CleanModelContextProtocol.Application.Common.Exceptions;
using CleanModelContextProtocol.Presentation.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Shouldly;
using Xunit;

public sealed class ToolErrorFilterTests : IDisposable
{
    private readonly ServiceProvider services = new ServiceCollection().AddFakeLogging().BuildServiceProvider();

    public void Dispose()
    {
        this.services.Dispose();
    }

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
        result.Content.OfType<TextContentBlock>().Single().Text.ShouldBe("Not Found: Expected Exception Check the ids: movies_search, authors_search and reviews_list return valid ones.");
    }

    [Fact]
    public async Task Filter_ShouldLog_UnexpectedException()
    {
        // Arrange
        var exception = new InvalidOperationException("Broken");
        var (request, collector) = this.CreateRequest();
        var handler = ToolErrorFilter.Filter((request, cancellationToken) => throw exception);

        // Act
        var result = await handler(request, CancellationToken.None);

        // Assert
        result.IsError.ShouldBe(true);

        var record = collector.LatestRecord;

        collector.Count.ShouldBe(1);
        record.Level.ShouldBe(LogLevel.Error);
        record.Exception.ShouldBe(exception);
        record.Category.ShouldBe(typeof(ToolErrorFilter).FullName);
        record.Message.ShouldBe("An unexpected exception stopped the movies_search tool");
    }

    [Fact]
    public async Task Filter_ShouldNotLog_ExpectedException()
    {
        // Arrange
        var (request, collector) = this.CreateRequest();
        var handler = ToolErrorFilter.Filter((request, cancellationToken) => throw new NotFoundException("Expected Exception"));

        // Act
        var result = await handler(request, CancellationToken.None);

        // Assert
        result.IsError.ShouldBe(true);
        collector.Count.ShouldBe(0);
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
        result.Content.OfType<TextContentBlock>().Single().Text.ShouldBe("Not Found: Expected Exception Check the ids: movies_search, authors_search and reviews_list return valid ones.");
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

    private (RequestContext<CallToolRequestParams> Request, FakeLogCollector Collector) CreateRequest()
    {
        var request = new RequestContext<CallToolRequestParams>(
            Substitute.For<McpServer>(),
            new JsonRpcRequest { Method = RequestMethods.ToolsCall },
            new CallToolRequestParams { Name = "movies_search" })
        {
            Services = this.services
        };

        return (request, this.services.GetFakeLogCollector());
    }
}
