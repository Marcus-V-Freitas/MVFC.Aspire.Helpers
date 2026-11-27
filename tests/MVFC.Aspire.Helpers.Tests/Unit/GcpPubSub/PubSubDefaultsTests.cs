namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubDefaultsTests
{
    [Theory]
    [InlineData(true, "172.17.0.1")]
    [InlineData(false, "host.docker.internal")]
    public void GetDockerInternalHost_ShouldSelectHostForExplicitPlatform(bool isLinux, string expected)
    {
        // Arrange
        var platform = isLinux;

        // Act
        var result = PubSubDefaults.GetDockerInternalHost(platform);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DockerInternalHost_ShouldReturnOperatingSystemSpecificHost()
    {
        // Arrange
        var expected = OperatingSystem.IsLinux() ? "172.17.0.1" : "host.docker.internal";

        // Act
        var result = PubSubDefaults.DockerInternalHost;

        // Assert
        result.Should().Be(expected);
    }
}
