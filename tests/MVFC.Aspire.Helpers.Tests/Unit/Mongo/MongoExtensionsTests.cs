namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public sealed class MongoExtensionsTests
{
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
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
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
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        var mongoBuilder = appBuilder.AddMongoReplicaSet("mongo");

        // Act
        var act = () => project.WithReference(mongoBuilder);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithReference_WithDumps_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        IReadOnlyCollection<IMongoClassDump> dumps =
        [
            new NoOpMongoDump("db", "collection", 1)
        ];
        var mongoBuilder = appBuilder.AddMongoReplicaSet("mongo").WithDumps(dumps);

        // Act
        var act = () => project.WithReference(mongoBuilder);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithReference_CalledTwice_ShouldNotRegisterDumpsExecutorTwice()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project1 = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api1");
        var project2 = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api2");
        IReadOnlyCollection<IMongoClassDump> dumps =
        [
            new NoOpMongoDump("db", "collection", 1)
        ];
        var mongoBuilder = appBuilder.AddMongoReplicaSet("mongo").WithDumps(dumps);

        // Act
        project1.WithReference(mongoBuilder);
        project2.WithReference(mongoBuilder);

        // Assert
        mongoBuilder.Resource.Annotations.OfType<MongoDumpsExecutedAnnotation>().Should().ContainSingle();
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
