namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubDefaultsTests
{
    [Fact]
    public void DockerInternalHost_ShouldReturnOperatingSystemSpecificHost()
    {
        // Act
        var result = PubSubDefaults.DockerInternalHost;

        // Assert
        if (OperatingSystem.IsLinux())
            result.Should().Be("172.17.0.1");
        else
            result.Should().Be("host.docker.internal");
    }
}
