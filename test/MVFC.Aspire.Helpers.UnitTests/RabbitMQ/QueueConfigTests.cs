namespace MVFC.Aspire.Helpers.UnitTests.RabbitMQ;

public sealed class QueueConfigTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var config = new QueueConfig("test-queue");

        config.Name.Should().Be("test-queue");
        config.ExchangeName.Should().BeNull();
        config.RoutingKey.Should().BeNull();
        config.Durable.Should().BeTrue();
        config.AutoDelete.Should().BeFalse();
        config.DeadLetterExchange.Should().BeNull();
        config.MessageTTL.Should().BeNull();
    }

    [Fact]
    public void CustomValues_ShouldOverrideDefaults()
    {
        var config = new QueueConfig(
            Name: "test-queue",
            ExchangeName: "test-exchange",
            RoutingKey: "test.key",
            Durable: false,
            AutoDelete: true,
            DeadLetterExchange: "dlx",
            MessageTTL: 60000);

        config.Name.Should().Be("test-queue");
        config.ExchangeName.Should().Be("test-exchange");
        config.RoutingKey.Should().Be("test.key");
        config.Durable.Should().BeFalse();
        config.AutoDelete.Should().BeTrue();
        config.DeadLetterExchange.Should().Be("dlx");
        config.MessageTTL.Should().Be(60000);
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        var config1 = new QueueConfig("test-queue", "test-exchange");
        var config2 = new QueueConfig("test-queue", "test-exchange");

        config1.Should().Be(config2);
    }
}
