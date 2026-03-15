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
}
