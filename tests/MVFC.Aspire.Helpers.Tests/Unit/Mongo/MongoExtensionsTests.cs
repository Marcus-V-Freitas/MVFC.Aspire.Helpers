namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public sealed class MongoExtensionsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithReference_WhenNoDumpsAreConfigured_ShouldCompleteReadinessWithoutResolvingEndpoint(bool emptyDumps)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var project = builder.AddTestProject("api");
        var mongo = builder.AddMongoReplicaSet("mongo");

        if (emptyDumps)
            mongo.WithDumps([]);

        project.WithReference(mongo);
        using var services = new ServiceCollection().BuildServiceProvider();

        // Act
        var act = () => builder.Eventing.PublishAsync(new ResourceReadyEvent(project.Resource, services), TestContext.Current.CancellationToken);

        // Assert
        await act.Should().NotThrowAsync();
        mongo.Resource.MongoEndpoint.IsAllocated.Should().BeFalse();
    }

    [Fact]
    public void AddMongoReplicaSet_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddMongoReplicaSet("mongo");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddMongoReplicaSet_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddMongoReplicaSet(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddMongoReplicaSet_ShouldConfigureResource_WithDefaults()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var mongoBuilder = builder.AddMongoReplicaSet("mongo-replica");

        // Assert
        mongoBuilder.Should().NotBeNull();
        mongoBuilder.Resource.Name.Should().Be("mongo-replica");

        // Garante que o endpoint foi criado com o nome esperado.
        var endpoint = mongoBuilder.Resource.MongoEndpoint;
        endpoint.EndpointName.Should().Be("mongodb");
    }

    [Fact]
    public void WithDataVolume_ShouldNotThrow_WhenVolumeNameIsValid()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var mongoBuilder = builder.AddMongoReplicaSet("mongo");

        // Act
        var act = () => mongoBuilder.WithDataVolume("mongo-data");

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void WithDataVolume_ShouldThrow_WhenVolumeNameIsInvalid(string? volumeName)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var mongoBuilder = builder.AddMongoReplicaSet("mongo");

        // Act
        var act = () => mongoBuilder.WithDataVolume(volumeName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithDumps_ShouldNotThrow_WhenDumpsCollectionIsProvided()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var mongoBuilder = builder.AddMongoReplicaSet("mongo");

        IReadOnlyCollection<IMongoClassDump> dumps =
        [
            new NoOpMongoDump("db", "collection", 1),
            new NoOpMongoDump("db2", "collection2", 5)
        ];

        // Act
        var act = () => mongoBuilder.WithDumps(dumps);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddMongoReplicaSet_ShouldThrow_WhenPortIsInvalid()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddMongoReplicaSet("mongo", port: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MongoReplicaSetResource>? builder = null;

        // Act
        var act = () => builder!.WithDockerImage("img", "tag");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "tag")]
    [InlineData("", "tag")]
    [InlineData("img", null)]
    [InlineData("img", "")]
    public void WithDockerImage_ShouldThrow_WhenImageOrTagInvalid(string? image, string? tag)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mongoBuilder = appBuilder.AddMongoReplicaSet("mongo");

        // Act
        var act = () => mongoBuilder.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenProjectIsNull()
    {
        // Arrange
        IResourceBuilder<ProjectResource>? project = null;
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var mongoBuilder = appBuilder.AddMongoReplicaSet("mongo");

        // Act
        var act = () => project!.WithReference(mongoBuilder);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenMongoBuilderIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddTestProject("api");
        IResourceBuilder<MongoReplicaSetResource>? mongoBuilder = null;

        // Act
        var act = () => project.WithReference(mongoBuilder!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_WithoutDumps_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddTestProject("api");
        var mongoBuilder = appBuilder.AddMongoReplicaSet("mongo");

        // Act
        var act = () => project.WithReference(mongoBuilder);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WithReference_ShouldExecuteDumpsOnce_RegardlessOfConfigurationOrder(bool configureDumpsFirst)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project1 = appBuilder.AddTestProject("api1");
        var project2 = appBuilder.AddTestProject("api2");
        var mongoBuilder = appBuilder.AddMongoReplicaSet("mongo");
        var endpoint = mongoBuilder.Resource.Annotations.OfType<EndpointAnnotation>().Single();
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 27017);
        var dump = new Mock<IMongoClassDump>();
        var cancellationToken = TestContext.Current.CancellationToken;
        dump.Setup(d => d.ExecuteDumpAsync(It.IsAny<IMongoClient>(), cancellationToken)).Returns(Task.CompletedTask);

        if (configureDumpsFirst)
            mongoBuilder.WithDumps(dump.Object);

        project1.WithReference(mongoBuilder);
        project2.WithReference(mongoBuilder);

        if (!configureDumpsFirst)
            mongoBuilder.WithDumps(dump.Object);

        using var services = new ServiceCollection().BuildServiceProvider();

        // Act
        await appBuilder.Eventing.PublishAsync(new ResourceReadyEvent(project1.Resource, services), cancellationToken);
        await appBuilder.Eventing.PublishAsync(new ResourceReadyEvent(project2.Resource, services), cancellationToken);

        // Assert
        dump.Verify(d => d.ExecuteDumpAsync(It.IsAny<IMongoClient>(), cancellationToken), Times.Once);
    }

    [Fact]
    public void WithDumps_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MongoReplicaSetResource>? builder = null;
        IReadOnlyCollection<IMongoClassDump> dumps = [new NoOpMongoDump("db", "col", 1)];

        // Act
        var act = () => builder!.WithDumps(dumps);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithDataVolume_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<MongoReplicaSetResource>? builder = null;

        // Act
        var act = () => builder!.WithDataVolume("vol");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
