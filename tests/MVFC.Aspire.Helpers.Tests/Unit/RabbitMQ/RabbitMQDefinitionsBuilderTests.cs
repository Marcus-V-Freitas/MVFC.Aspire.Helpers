namespace MVFC.Aspire.Helpers.Tests.Unit.RabbitMQ;

public sealed class RabbitMQDefinitionsBuilderTests
{
    [Fact]
    public void AddRabbitMQ_WithExchangesAndQueues_ShouldAccumulateOnResource()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");

        IReadOnlyList<ExchangeConfig> exchanges =
        [
            new("ex-direct", "direct"),
            new("ex-topic", "topic", Durable: false),
            new("ex-fanout", "fanout", AutoDelete: true)
        ];

        IReadOnlyList<QueueConfig> queues =
        [
            new("q1", "ex-direct", "key.one"),
            new("q2", "ex-topic", "key.*", DeadLetterExchange: "dlx"),
            new("q3", "ex-fanout", MessageTTL: 5000),
            new("q4-no-exchange")
        ];

        // Act
        rb.WithExchanges(exchanges).WithQueues(queues);

        // Assert
        rb.Resource.Exchanges.Should().HaveCount(3);
        rb.Resource.Exchanges!.Select(e => e.Name)
            .Should().BeEquivalentTo(new[] { "ex-direct", "ex-topic", "ex-fanout" });

        rb.Resource.Queues.Should().HaveCount(4);
        rb.Resource.Queues!.Select(q => q.Name)
            .Should().BeEquivalentTo(new[] { "q1", "q2", "q3", "q4-no-exchange" });
    }

    [Fact]
    public void AddRabbitMQ_WithNoExchangesOrQueues_ShouldLeaveCollectionsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");

        // Assert
        rb.Resource.Exchanges.Should().BeNull();
        rb.Resource.Queues.Should().BeNull();
    }

    [Fact]
    public void WithExchanges_CalledTwice_ShouldAccumulateAllExchanges()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");

        // Act
        rb.WithExchanges([new("ex-1", "direct")]);
        rb.WithExchanges([new("ex-2", "topic"), new("ex-3", "fanout")]);

        // Assert
        rb.Resource.Exchanges.Should().HaveCount(3);
    }

    [Fact]
    public void WithQueues_CalledTwice_ShouldAccumulateAllQueues()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");

        // Act
        rb.WithQueues([new("q1")]);
        rb.WithQueues([new("q2"), new("q3")]);

        // Assert
        rb.Resource.Queues.Should().HaveCount(3);
    }

    [Fact]
    public void ExchangeConfig_ShouldHaveCorrectDefaults()
    {
        // Act
        var exchange = new ExchangeConfig("my-exchange");

        // Assert
        exchange.Name.Should().Be("my-exchange");
        exchange.Type.Should().Be("direct");
        exchange.Durable.Should().BeTrue();
        exchange.AutoDelete.Should().BeFalse();
    }

    [Fact]
    public void QueueConfig_ShouldHaveCorrectDefaults()
    {
        // Act
        var queue = new QueueConfig("my-queue");

        // Assert
        queue.Name.Should().Be("my-queue");
        queue.ExchangeName.Should().BeNull();
        queue.RoutingKey.Should().BeNull();
        queue.Durable.Should().BeTrue();
        queue.AutoDelete.Should().BeFalse();
        queue.DeadLetterExchange.Should().BeNull();
        queue.MessageTTL.Should().BeNull();
    }

    [Fact]
    public void QueueConfig_ShouldHoldAllOptionalFields()
    {
        // Act
        var queue = new QueueConfig(
            Name: "dlq",
            ExchangeName: "dlx",
            RoutingKey: "dead.*",
            Durable: false,
            AutoDelete: true,
            DeadLetterExchange: "retry-exchange",
            MessageTTL: 3000);

        // Assert
        queue.DeadLetterExchange.Should().Be("retry-exchange");
        queue.MessageTTL.Should().Be(3000);
        queue.AutoDelete.Should().BeTrue();
        queue.Durable.Should().BeFalse();
        queue.RoutingKey.Should().Be("dead.*");
    }
}
