namespace CleanModelContextProtocol.Presentation.Tests.Unit.Authentication;

using CleanModelContextProtocol.Presentation.Authentication;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

public class AuthenticationSettingsTests
{
    [Fact]
    public void FromConfiguration_ShouldReturn_DisabledSettings_WhenSectionIsMissing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = AuthenticationSettings.FromConfiguration(configuration);

        // Assert
        result.Enabled.ShouldBeFalse();
        result.Scopes.ShouldBeEmpty();
    }

    [Fact]
    public void FromConfiguration_ShouldReturn_EnabledSettings_WhenAuthorityIsSet()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Authentication:Authority"] = "https://login.example.com",
                ["Authentication:Audience"] = "https://localhost:5001",
                ["Authentication:Scopes:0"] = "movies.read",
            })
            .Build();

        // Act
        var result = AuthenticationSettings.FromConfiguration(configuration);

        // Assert
        result.Enabled.ShouldBeTrue();
        result.Authority.ShouldBe("https://login.example.com");
        result.Audience.ShouldBe("https://localhost:5001");
        result.Scopes.ShouldBe(["movies.read"]);
    }
}
