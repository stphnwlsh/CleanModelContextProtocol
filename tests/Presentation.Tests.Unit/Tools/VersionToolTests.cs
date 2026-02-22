namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

using CleanModelContextProtocol.Presentation.Tools;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;
using Entities = Application.Versions.Entities;
using Queries = Application.Versions.Queries;

public class VersionToolTests
{
    [Fact]
    public async Task GetVersion_ShouldReturn_MarkdownTable()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetVersion.GetVersionQuery>())
            .ReturnsForAnyArgs(new Entities.Version
            {
                FileVersion = "1.2.3.4",
                InformationalVersion = "5.6.7.8"
            });

        // Act
        var response = await VersionTools.GetVersion(sender);

        // Assert
        response.ShouldContain("| File Version | 1.2.3.4 |");
        response.ShouldContain("| Informational Version | 5.6.7.8 |");
    }

    [Fact]
    public async Task GetVersion_ShouldReturn_Error()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        _ = sender
            .Send(Arg.Any<Queries.GetVersion.GetVersionQuery>())
            .Throws(new ArgumentException("Expected Exception"));

        // Act
        var response = await VersionTools.GetVersion(sender);

        // Assert
        response.ShouldBe("Error: Expected Exception");
    }
}
