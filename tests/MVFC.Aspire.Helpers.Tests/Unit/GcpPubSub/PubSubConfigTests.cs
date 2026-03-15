namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubConfigTests
{
    [Fact]
    public void Constructor_WithProjectId_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var config = new PubSubConfig("my-project");

        // Assert
        config.ProjectId.Should().Be("my-project");
        config.MessageConfigs.Should().BeEmpty();
        config.UpDelay.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithSingleMessageConfig_ShouldInitializeCorrectly()
    {
        // Arrange
        var messageConfig = new MessageConfig("topic", "sub");

        // Act
        var config = new PubSubConfig("my-project", messageConfig, secondsDelay: 10);

        // Assert
        config.ProjectId.Should().Be("my-project");
        config.MessageConfigs.Should().HaveCount(1);
        config.MessageConfigs[0].As<MessageConfig>().TopicName.Should().Be("topic");
        config.UpDelay.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Constructor_WithMultipleMessageConfigs_ShouldInitializeCorrectly()
    {
        // Arrange
        var configs = new List<MessageConfig>
        {
            new("t1", "s1"),
            new("t2", "s2")
        };

        // Act
        var config = new PubSubConfig("my-project", secondsDelay: 3, messageConfigs: configs);

        // Assert
        config.ProjectId.Should().Be("my-project");
        config.MessageConfigs.Should().HaveCount(2);
        config.UpDelay.Should().Be(TimeSpan.FromSeconds(3));
    }
}
