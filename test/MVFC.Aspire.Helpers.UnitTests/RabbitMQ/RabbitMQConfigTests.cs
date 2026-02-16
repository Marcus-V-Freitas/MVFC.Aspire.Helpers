namespace MVFC.Aspire.Helpers.UnitTests.RabbitMQ;

public sealed class RabbitMQConfigTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var config = new RabbitMQConfig();

        config.Port.Should().BeNull();
        config.ManagementPort.Should().BeNull();
        config.Username.Should().Be("guest");
        config.Password.Should().Be("guest");
        config.Exchanges.Should().BeNull();
        config.Queues.Should().BeNull();
        config.ImageName.Should().Be("rabbitmq");
        config.ImageTag.Should().Be("3-management");
        config.VolumeName.Should().BeNull();
    }

    [Fact]
    public void CustomValues_ShouldOverrideDefaults()
    {
        var exchanges = new List<ExchangeConfig> {
            new("test-exchange", "topic")
        };

        var queues = new List<QueueConfig> {
            new("test-queue", "test-exchange")
        };

        var config = new RabbitMQConfig(
            Port: 5672,
            ManagementPort: 15672,
            Username: "admin",
            Password: "secret",
            Exchanges: exchanges,
            Queues: queues,
            ImageName: "rabbitmq",
            ImageTag: "3-management-alpine",
            VolumeName: "rabbitmq-data");

        config.Port.Should().Be(5672);
        config.ManagementPort.Should().Be(15672);
        config.Username.Should().Be("admin");
        config.Password.Should().Be("secret");
        config.Exchanges.Should().HaveCount(1);
        config.Queues.Should().HaveCount(1);
        config.ImageName.Should().Be("rabbitmq");
        config.ImageTag.Should().Be("3-management-alpine");
        config.VolumeName.Should().Be("rabbitmq-data");
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        var config1 = new RabbitMQConfig(Port: 5672);
        var config2 = new RabbitMQConfig(Port: 5672);

        config1.Should().Be(config2);
    }
}
