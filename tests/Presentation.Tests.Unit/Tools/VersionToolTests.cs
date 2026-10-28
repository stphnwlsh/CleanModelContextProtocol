namespace CleanModelContextProtocol.Presentation.Tests.Unit.Tools;

using CleanModelContextProtocol.Presentation.Tools;
using Mediator;
using NSubstitute;
using Shouldly;
using Xunit;
using Entities = Application.Versions.Entities;
using Queries = Application.Versions.Queries;

public class VersionToolTests
{
    [Fact]
    public async Task GetVersion_ShouldReturn_Version()
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
        response.FileVersion.ShouldBe("1.2.3.4");
        response.InformationalVersion.ShouldBe("5.6.7.8");
    }
}
