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
            .Should().BeEquivalentTo(["ex-direct", "ex-topic", "ex-fanout"]);

        rb.Resource.Queues.Should().HaveCount(4);
        rb.Resource.Queues!.Select(q => q.Name)
            .Should().BeEquivalentTo(["q1", "q2", "q3", "q4-no-exchange"]);
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

    [Fact]
    public void Build_WithExchangesAndQueues_ShouldProduceCorrectDefinitions()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");
        rb.WithExchanges([new("ex-direct", "direct")]);
        rb.WithQueues([new("q1", "ex-direct", "key.one")]);

        // Act
        var defs = RabbitMQDefinitionsBuilder.Build(rb.Resource);

        // Assert
        defs.Exchanges.Should().ContainSingle(e => e.Name == "ex-direct");
        defs.Queues.Should().ContainSingle(q => q.Name == "q1");
        defs.Bindings.Should().ContainSingle(b => b.Source == "ex-direct" && b.Destination == "q1");
    }

    [Fact]
    public void Build_WithoutExchanges_ShouldReturnEmptyExchangeList()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");
        rb.WithQueues([new("q1")]);

        // Act
        var defs = RabbitMQDefinitionsBuilder.Build(rb.Resource);

        // Assert
        defs.Exchanges.Should().BeEmpty();
    }

    [Fact]
    public void Build_WithoutQueues_ShouldReturnEmptyQueuesAndBindings()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");
        rb.WithExchanges([new("ex-1", "direct")]);

        // Act
        var defs = RabbitMQDefinitionsBuilder.Build(rb.Resource);

        // Assert
        defs.Queues.Should().BeEmpty();
        defs.Bindings.Should().BeEmpty();
    }

    [Fact]
    public void Build_WithNoExchangesOrQueues_ShouldReturnEmptyCollections()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");

        // Act
        var defs = RabbitMQDefinitionsBuilder.Build(rb.Resource);

        // Assert
        defs.Exchanges.Should().BeEmpty();
        defs.Queues.Should().BeEmpty();
        defs.Bindings.Should().BeEmpty();
        defs.Users.Should().ContainSingle();
        defs.Vhosts.Should().ContainSingle();
        defs.Permissions.Should().ContainSingle();
    }

    [Fact]
    public void Build_QueueWithoutExchange_ShouldNotProduceBinding()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");
        rb.WithQueues([new("q-standalone")]);

        // Act
        var defs = RabbitMQDefinitionsBuilder.Build(rb.Resource);

        // Assert
        defs.Queues.Should().ContainSingle(q => q.Name == "q-standalone");
        defs.Bindings.Should().BeEmpty();
    }

    [Fact]
    public void Build_QueueWithExchangeButNoRoutingKey_ShouldUseQueueNameAsRoutingKey()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");
        rb.WithQueues([new("my-queue", ExchangeName: "my-exchange")]);

        // Act
        var defs = RabbitMQDefinitionsBuilder.Build(rb.Resource);

        // Assert
        defs.Bindings.Should().ContainSingle(b => b.RoutingKey == "my-queue");
    }

    [Fact]
    public void Build_QueueWithDeadLetterExchange_ShouldIncludeArgument()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");
        rb.WithQueues([new("q1", DeadLetterExchange: "dlx")]);

        // Act
        var defs = RabbitMQDefinitionsBuilder.Build(rb.Resource);

        // Assert
        var queue = defs.Queues.First(q => q.Name == "q1");
        queue.Arguments.Should().ContainKey("x-dead-letter-exchange");
        queue.Arguments["x-dead-letter-exchange"].Should().Be("dlx");
    }

    [Fact]
    public void Build_QueueWithMessageTTL_ShouldIncludeArgument()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");
        rb.WithQueues([new("q1", MessageTTL: 5000)]);

        // Act
        var defs = RabbitMQDefinitionsBuilder.Build(rb.Resource);

        // Assert
        var queue = defs.Queues.First(q => q.Name == "q1");
        queue.Arguments.Should().ContainKey("x-message-ttl");
        queue.Arguments["x-message-ttl"].Should().Be(5000);
    }

    [Fact]
    public void Build_QueueWithNoArguments_ShouldHaveEmptyArguments()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");
        rb.WithQueues([new("q-simple")]);

        // Act
        var defs = RabbitMQDefinitionsBuilder.Build(rb.Resource);

        // Assert
        var queue = defs.Queues.First(q => q.Name == "q-simple");
        queue.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void Build_ShouldHashPasswordAsBase64()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rb = appBuilder.AddRabbitMQ("rabbit");

        // Act
        var defs = RabbitMQDefinitionsBuilder.Build(rb.Resource);

        // Assert
        var user = defs.Users.First();
        user.PasswordHash.Should().NotBeNullOrWhiteSpace();
        var bytes = Convert.FromBase64String(user.PasswordHash);
        bytes.Length.Should().Be(36); // 4 bytes salt + 32 bytes SHA256
        user.HashingAlgorithm.Should().Be("rabbit_password_hashing_sha256");
    }
}

