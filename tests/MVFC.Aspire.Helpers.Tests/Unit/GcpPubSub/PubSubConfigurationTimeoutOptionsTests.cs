namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubConfigurationTimeoutOptionsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void WithConfigurationTimeout_WhenInvalid_ShouldRejectValue(int seconds)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var pubSub = builder.AddGcpPubSub("pubsub");

        // Act
        var act = () => pubSub.WithConfigurationTimeout(seconds);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WithConfigurationTimeout_ShouldPreserveDefaultsAndResourceIsolation()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var first = builder.AddGcpPubSub("first");
        var second = builder.AddGcpPubSub("second");

        // Act
        var result = first.WithConfigurationTimeout(120);

        // Assert
        result.Should().BeSameAs(first);
        first.Resource.ConfigurationTimeoutSeconds.Should().Be(120);
        second.Resource.ConfigurationTimeoutSeconds.Should().Be(60);
        first.Resource.WaitTimeoutSeconds.Should().Be(PubSubDefaults.WAIT_TIMEOUT_SECONDS_DEFAULT);
    }

    [Fact]
    public void WithConfigurationTimeout_WhenBuilderIsNull_ShouldRejectConfiguration()
    {
        // Arrange
        IResourceBuilder<PubSubEmulatorResource> builder = null!;

        // Act
        var act = () => builder.WithConfigurationTimeout(60);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
