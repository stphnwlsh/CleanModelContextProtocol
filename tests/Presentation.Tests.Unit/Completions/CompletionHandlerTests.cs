namespace CleanModelContextProtocol.Presentation.Tests.Unit.Completions;

using Application.Common.Paging;
using Application.Reviews.Entities;
using Application.Reviews.Queries.GetReviews;
using Mediator;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Presentation.Completions;
using Shouldly;
using Xunit;

public class CompletionHandlerTests
{
    [Fact]
    public async Task Complete_WithoutRequest_ShouldReturn_Nothing()
    {
        // Act
        var result = await CompletionHandler.Complete(Substitute.For<ISender>(), null, CancellationToken.None);

        // Assert
        result.Completion.Values.ShouldBeEmpty();
    }

    [Fact]
    public async Task Complete_ReviewWithoutMovie_ShouldNot_Match()
    {
        // Arrange
        var review = new Review(Guid.NewGuid(), 5);
        var sender = Substitute.For<ISender>();

        _ = sender.Send(Arg.Any<GetReviewsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new Page<Review>([review], 0, Paging.MaxLimit, 1));

        var request = new CompleteRequestParams
        {
            Ref = new ResourceTemplateReference { Uri = "reviews://{reviewId}" },
            Argument = new Argument { Name = "reviewId", Value = "no-match" }
        };

        // Act
        var result = await CompletionHandler.Complete(sender, request, CancellationToken.None);

        // Assert
        result.Completion.Values.ShouldBeEmpty();
    }
}
