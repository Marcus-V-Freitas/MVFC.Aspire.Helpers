namespace MVFC.Aspire.Helpers.Tests.Unit.RabbitMQ;

public sealed class RabbitMQExtensionsTests
{
    [Fact]
    public void AddRabbitMQ_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddRabbitMQ("rabbit");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddRabbitMQ_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddRabbitMQ(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddRabbitMQ_ShouldConfigureResource_WithDefaultsAndNotThrow()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var resourceBuilder = builder.AddRabbitMQ("rabbit");

        // Assert
        resourceBuilder.Should().NotBeNull();
        resourceBuilder.Resource.Name.Should().Be("rabbit");
        resourceBuilder.Resource.Username.Should().NotBeNullOrWhiteSpace();
        resourceBuilder.Resource.Password.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<RabbitMQResource>? builder = null;

        // Act
        var act = () => builder!.WithDockerImage("rabbitmq", "management");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "tag")]
    [InlineData("", "tag")]
    [InlineData(" ", "tag")]
    [InlineData("image", null)]
    [InlineData("image", "")]
    [InlineData("image", " ")]
    public void WithDockerImage_ShouldThrow_WhenImageOrTagInvalid(string? image, string? tag)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rabbitBuilder = appBuilder.AddRabbitMQ("rabbit");

        // Act
        var act = () => rabbitBuilder.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithDockerImage_ShouldNotThrow_WhenParametersAreValid()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rabbitBuilder = appBuilder.AddRabbitMQ("rabbit");

        // Act
        var act = () => rabbitBuilder.WithDockerImage("rabbitmq", "3-management");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithCredentials_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<RabbitMQResource>? builder = null;

        // Act
        var act = () => builder!.WithCredentials("user", "pass");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "pass")]
    [InlineData("", "pass")]
    [InlineData(" ", "pass")]
    [InlineData("user", null)]
    [InlineData("user", "")]
    [InlineData("user", " ")]
    public void WithCredentials_ShouldThrow_WhenUsernameOrPasswordInvalid(string? user, string? pass)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rabbitBuilder = appBuilder.AddRabbitMQ("rabbit");

        // Act
        var act = () => rabbitBuilder.WithCredentials(user!, pass!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithCredentials_ShouldSetUsernameAndPassword()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rabbitBuilder = appBuilder.AddRabbitMQ("rabbit");

        // Act
        rabbitBuilder.WithCredentials("admin", "Secret123!");

        // Assert
        rabbitBuilder.Resource.Username.Should().Be("admin");
        rabbitBuilder.Resource.Password.Should().Be("Secret123!");
    }

    [Fact]
    public void WithExchanges_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<RabbitMQResource>? builder = null;
        IReadOnlyList<ExchangeConfig> exchanges =
        [
            new("exchange", "topic", true, false)
        ];

        // Act
        var act = () => builder!.WithExchanges(exchanges);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithExchanges_ShouldAppendExchanges()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rabbitBuilder = appBuilder.AddRabbitMQ("rabbit");

        IReadOnlyList<ExchangeConfig> exchanges =
        [
            new("exchange-1", "topic", true, false),
            new("exchange-2", "fanout", false, true)
        ];

        // Act
        rabbitBuilder.WithExchanges(exchanges);

        // Assert
        rabbitBuilder.Resource.Exchanges.Should().NotBeNull();
        rabbitBuilder.Resource.Exchanges.Should().HaveCount(2);
        rabbitBuilder.Resource.Exchanges!.Select(e => e.Name).Should().Contain(["exchange-1", "exchange-2"]);
    }

    [Fact]
    public void WithQueues_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<RabbitMQResource>? builder = null;
        IReadOnlyList<QueueConfig> queues =
        [
            new("queue-1", "exchange-1", "routing"),
        ];

        // Act
        var act = () => builder!.WithQueues(queues);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithQueues_ShouldAppendQueues()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rabbitBuilder = appBuilder.AddRabbitMQ("rabbit");

        IReadOnlyList<QueueConfig> queues =
        [
            new("queue-1", "exchange-1", "routing-1"),
            new("queue-2", "exchange-2", "routing-2", AutoDelete: true, DeadLetterExchange: "dlx", MessageTTL: 1000)
        ];

        // Act
        rabbitBuilder.WithQueues(queues);

        // Assert
        rabbitBuilder.Resource.Queues.Should().NotBeNull();
        rabbitBuilder.Resource.Queues.Should().HaveCount(2);
        rabbitBuilder.Resource.Queues!.Select(q => q.Name).Should().Contain(["queue-1", "queue-2"]);
    }

    [Fact]
    public void WithDataVolume_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<RabbitMQResource>? builder = null;

        // Act
        var act = () => builder!.WithDataVolume("rabbit-data");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WithDataVolume_ShouldThrow_WhenVolumeNameInvalid(string? volumeName)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rabbitBuilder = appBuilder.AddRabbitMQ("rabbit");

        // Act
        var act = () => rabbitBuilder.WithDataVolume(volumeName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithDataVolume_ShouldNotThrow_WhenVolumeNameIsValid()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rabbitBuilder = appBuilder.AddRabbitMQ("rabbit");

        // Act
        var act = () => rabbitBuilder.WithDataVolume("rabbit-data");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenProjectIsNull()
    {
        // Arrange
        IResourceBuilder<ProjectResource>? project = null;
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var rabbitBuilder = appBuilder.AddRabbitMQ("rabbit");

        // Act
        var act = () => project!.WithReference(rabbitBuilder);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenRabbitMQBuilderIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        IResourceBuilder<RabbitMQResource>? rabbitBuilder = null;

        // Act
        var act = () => project.WithReference(rabbitBuilder!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldNotThrow_WhenParametersAreValid()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        var rabbitBuilder = appBuilder.AddRabbitMQ("rabbit");

        // Act
        var act = () => project.WithReference(rabbitBuilder);

        // Assert
        act.Should().NotThrow();
    }
}
