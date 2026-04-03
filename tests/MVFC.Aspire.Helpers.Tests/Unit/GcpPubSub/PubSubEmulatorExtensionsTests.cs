namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubEmulatorExtensionsTests
{
    [Fact]
    public void AddGcpPubSub_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddGcpPubSub("pubsub");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddGcpPubSub_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGcpPubSub(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGcpPubSub_ShouldThrow_WhenPortIsInvalid()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGcpPubSub("pubsub", -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddGcpPubSub_ShouldSetResourceName()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var pubSub = builder.AddGcpPubSub("pubsub");

        // Assert
        pubSub.Resource.Name.Should().Be("pubsub");
    }

    [Fact]
    public void AddGcpPubSub_ShouldInitializeWithEmptyConfigs()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var pubSub = builder.AddGcpPubSub("pubsub");

        // Assert
        pubSub.Resource.PubSubConfigs.Should().BeEmpty();
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<PubSubEmulatorResource>? builder = null;

        // Act
        var act = () => builder!.WithDockerImage("img", "tag");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "latest")]
    [InlineData("", "latest")]
    [InlineData(" ", "latest")]
    [InlineData("img", null)]
    [InlineData("img", "")]
    [InlineData("img", " ")]
    public void WithDockerImage_ShouldThrow_WhenImageOrTagInvalid(string? image, string? tag)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var pubSub = appBuilder.AddGcpPubSub("pubsub");

        // Act
        var act = () => pubSub.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithPubSubConfigs_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<PubSubEmulatorResource>? builder = null;
        var config = new PubSubConfig("my-project", new MessageConfig("my-topic"));

        // Act
        var act = () => builder!.WithPubSubConfigs(config);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithPubSubConfigs_ShouldAddSingleConfig()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var pubSub = appBuilder.AddGcpPubSub("pubsub");
        var config = new PubSubConfig("project-a", new MessageConfig("topic-a", "sub-a"));

        // Act
        pubSub.WithPubSubConfigs(config);

        // Assert
        pubSub.Resource.PubSubConfigs.Should().HaveCount(1);
        pubSub.Resource.PubSubConfigs[0].ProjectId.Should().Be("project-a");
    }

    [Fact]
    public void WithPubSubConfigs_ShouldAddMultipleConfigs()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var pubSub = appBuilder.AddGcpPubSub("pubsub");
        var config1 = new PubSubConfig("project-a", new MessageConfig("topic-a"));
        var config2 = new PubSubConfig("project-b", new MessageConfig("topic-b"));

        // Act
        pubSub.WithPubSubConfigs(config1, config2);

        // Assert
        pubSub.Resource.PubSubConfigs.Should().HaveCount(2);
        pubSub.Resource.PubSubConfigs.Select(c => c.ProjectId)
            .Should().BeEquivalentTo(["project-a", "project-b"]);
    }

    [Fact]
    public void WithPubSubConfigs_ShouldAccumulateOnChainedCalls()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var pubSub = appBuilder.AddGcpPubSub("pubsub");
        var config1 = new PubSubConfig("project-a", new MessageConfig("topic-a"));
        var config2 = new PubSubConfig("project-b", new MessageConfig("topic-b"));

        // Act
        pubSub
            .WithPubSubConfigs(config1)
            .WithPubSubConfigs(config2);

        // Assert
        pubSub.Resource.PubSubConfigs.Should().HaveCount(2);
    }

    [Fact]
    public void WithWaitTimeout_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<PubSubEmulatorResource>? builder = null;

        // Act
        var act = () => builder!.WithWaitTimeout(30);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithWaitTimeout_ShouldThrow_WhenValueIsNegative()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var pubSub = appBuilder.AddGcpPubSub("pubsub");

        // Act
        var act = () => pubSub.WithWaitTimeout(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(60)]
    public void WithWaitTimeout_ShouldSetWaitTimeoutSeconds(int seconds)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var pubSub = appBuilder.AddGcpPubSub("pubsub");

        // Act
        pubSub.WithWaitTimeout(seconds);

        // Assert
        pubSub.Resource.WaitTimeoutSeconds.Should().Be(seconds);
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenProjectIsNull()
    {
        // Arrange
        IResourceBuilder<ProjectResource>? project = null;
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var pubSub = appBuilder.AddGcpPubSub("pubsub");

        // Act
        var act = () => project!.WithReference(pubSub);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenPubSubBuilderIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        IResourceBuilder<PubSubEmulatorResource>? pubSub = null;

        // Act
        var act = () => project.WithReference(pubSub!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_WithConfigs_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        var pubSub = appBuilder.AddGcpPubSub("pubsub")
            .WithPubSubConfigs(new PubSubConfig("project-a", new MessageConfig("topic-a", "sub-a")));

        // Act
        var act = () => project.WithReference(pubSub);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithReference_WithoutConfigs_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        var pubSub = appBuilder.AddGcpPubSub("pubsub");

        // Act
        var act = () => project.WithReference(pubSub);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithReference_CalledTwice_ShouldNotRegisterConfiguratorTwice()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project1 = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api1");
        var project2 = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api2");
        var pubSub = appBuilder.AddGcpPubSub("pubsub")
            .WithPubSubConfigs(new PubSubConfig("project-a", new MessageConfig("topic-a", "sub-a")));

        // Act
        project1.WithReference(pubSub);
        project2.WithReference(pubSub);

        // Assert — annotation should be added only once
        pubSub.Resource.Annotations.OfType<PubSubConfiguredAnnotation>().Should().ContainSingle();
    }
}

