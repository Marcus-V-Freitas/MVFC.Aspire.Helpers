namespace MVFC.Aspire.Helpers.UnitTests.RabbitMQ;

public sealed class ExchangeConfigTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var config = new ExchangeConfig("test-exchange");

        config.Name.Should().Be("test-exchange");
        config.Type.Should().Be("direct");
        config.Durable.Should().BeTrue();
        config.AutoDelete.Should().BeFalse();
    }

    [Fact]
    public void CustomValues_ShouldOverrideDefaults()
    {
        var config = new ExchangeConfig(
            Name: "test-exchange",
            Type: "topic",
            Durable: false,
            AutoDelete: true);

        config.Name.Should().Be("test-exchange");
        config.Type.Should().Be("topic");
        config.Durable.Should().BeFalse();
        config.AutoDelete.Should().BeTrue();
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        var config1 = new ExchangeConfig("test-exchange", "topic");
        var config2 = new ExchangeConfig("test-exchange", "topic");

        config1.Should().Be(config2);
    }
}
