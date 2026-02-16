namespace MVFC.Aspire.Helpers.UnitTests.GcpPubSub;

public sealed class PubSubConfigTests
{
    [Fact]
    public void Constructor_WithSingleMessage_ShouldSetProperties()
    {
        var message = new MessageConfig(TopicName: "t1", SubscriptionName: "s1");
        var config = new PubSubConfig("my-project", message);

        config.ProjectId.Should().Be("my-project");
        config.MessageConfigs.Should().HaveCount(1);
        config.MessageConfigs[0].TopicName.Should().Be("t1");
        config.UpDelay.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithMultipleMessages_ShouldStoreAll()
    {
        var messages = new List<MessageConfig> {
            new(TopicName: "t1", SubscriptionName: "s1"),
            new(TopicName: "t2", SubscriptionName: "s2")
        };

        var config = new PubSubConfig("proj", messageConfigs: messages);

        config.MessageConfigs.Should().HaveCount(2);
        config.MessageConfigs[0].TopicName.Should().Be("t1");
        config.MessageConfigs[1].TopicName.Should().Be("t2");
    }

    [Fact]
    public void Constructor_WithNullMessages_ShouldDefaultToEmptyList()
    {
        var config = new PubSubConfig("proj");

        config.MessageConfigs.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithCustomDelay_ShouldSetUpDelay()
    {
        var config = new PubSubConfig("proj", secondsDelay: 10);

        config.UpDelay.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Constructor_SingleMessage_WithCustomDelay_ShouldSetBoth()
    {
        var message = new MessageConfig(TopicName: "t1");
        var config = new PubSubConfig("proj", message, secondsDelay: 15);

        config.MessageConfigs.Should().HaveCount(1);
        config.UpDelay.Should().Be(TimeSpan.FromSeconds(15));
    }
}
