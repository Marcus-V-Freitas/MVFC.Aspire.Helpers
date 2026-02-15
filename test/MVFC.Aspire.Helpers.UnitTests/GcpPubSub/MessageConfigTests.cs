using MVFC.Aspire.Helpers.GcpPubSub.Models;

namespace MVFC.Aspire.Helpers.UnitTests.GcpPubSub;

public class MessageConfigTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var config = new MessageConfig("my-topic");

        config.TopicName.Should().Be("my-topic");
        config.SubscriptionName.Should().BeNull();
        config.PushEndpoint.Should().BeNull();
        config.DeadLetterTopic.Should().BeNull();
        config.MaxDeliveryAttempts.Should().BeNull();
        config.AckDeadlineSeconds.Should().BeNull();
    }

    [Fact]
    public void AllValues_ShouldBeConfigurable()
    {
        var config = new MessageConfig(
            TopicName: "orders",
            SubscriptionName: "orders-sub",
            PushEndpoint: "/api/orders")
        {
            DeadLetterTopic = "orders-dlq",
            MaxDeliveryAttempts = 10,
            AckDeadlineSeconds = 60
        };

        config.TopicName.Should().Be("orders");
        config.SubscriptionName.Should().Be("orders-sub");
        config.PushEndpoint.Should().Be("/api/orders");
        config.DeadLetterTopic.Should().Be("orders-dlq");
        config.MaxDeliveryAttempts.Should().Be(10);
        config.AckDeadlineSeconds.Should().Be(60);
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        var config1 = new MessageConfig("t1", "s1");
        var config2 = new MessageConfig("t1", "s1");

        config1.Should().Be(config2);
    }

    [Fact]
    public void Equality_WithInitProperties_ShouldWorkCorrectly()
    {
        var config1 = new MessageConfig("t1") { DeadLetterTopic = "dlq", MaxDeliveryAttempts = 5 };
        var config2 = new MessageConfig("t1") { DeadLetterTopic = "dlq", MaxDeliveryAttempts = 5 };

        config1.Should().Be(config2);
    }
}
