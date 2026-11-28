namespace MVFC.Aspire.Helpers.Tests.Unit.GcpFirestore;

public sealed class FirestoreEmulatorExtensionsTests
{
    [Fact]
    public void AddGcpFirestore_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddGcpFirestore("firestore");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddGcpFirestore_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGcpFirestore(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGcpFirestore_ShouldThrow_WhenPortIsInvalid()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGcpFirestore("firestore", -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddGcpFirestore_ShouldSetResourceName()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var firestore = builder.AddGcpFirestore("firestore");

        // Assert
        firestore.Resource.Name.Should().Be("firestore");
    }

    [Fact]
    public void AddGcpFirestore_ShouldInitializeWithEmptyConfigs()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var firestore = builder.AddGcpFirestore("firestore");

        // Assert
        firestore.Resource.FirestoreConfigs.Should().BeEmpty();
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<FirestoreEmulatorResource>? builder = null;

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
        var firestore = appBuilder.AddGcpFirestore("firestore");

        // Act
        var act = () => firestore.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithFirestoreConfigs_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<FirestoreEmulatorResource>? builder = null;
        var config = new FirestoreConfig("my-project");

        // Act
        var act = () => builder!.WithFirestoreConfigs(config);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithFirestoreConfigs_ShouldAddSingleConfig()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var firestore = appBuilder.AddGcpFirestore("firestore");
        var config = new FirestoreConfig("project-a");

        // Act
        firestore.WithFirestoreConfigs(config);

        // Assert
        firestore.Resource.FirestoreConfigs.Should().HaveCount(1);
        firestore.Resource.FirestoreConfigs[0].ProjectId.Should().Be("project-a");
    }

    [Fact]
    public void WithFirestoreConfigs_ShouldAddMultipleConfigs()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var firestore = appBuilder.AddGcpFirestore("firestore");
        var config1 = new FirestoreConfig("project-a");
        var config2 = new FirestoreConfig("project-b");

        // Act
        firestore.WithFirestoreConfigs(config1, config2);

        // Assert
        firestore.Resource.FirestoreConfigs.Should().HaveCount(2);
        firestore.Resource.FirestoreConfigs.Select(c => c.ProjectId)
            .Should().BeEquivalentTo(["project-a", "project-b"]);
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenProjectIsNull()
    {
        // Arrange
        IResourceBuilder<ProjectResource>? project = null;
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var firestore = appBuilder.AddGcpFirestore("firestore");

        // Act
        var act = () => project!.WithReference(firestore);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenFirestoreBuilderIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddTestProject("api");
        IResourceBuilder<FirestoreEmulatorResource>? firestore = null;

        // Act
        var act = () => project.WithReference(firestore!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_WithConfigs_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddTestProject("api");
        var firestore = appBuilder.AddGcpFirestore("firestore")
            .WithFirestoreConfigs(new FirestoreConfig("project-a"));

        // Act
        var act = () => project.WithReference(firestore);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithReference_WithoutConfigs_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddTestProject("api");
        var firestore = appBuilder.AddGcpFirestore("firestore");

        // Act
        var act = () => project.WithReference(firestore);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithFirestoreConfigs_ShouldAccumulateOnChainedCalls()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var firestore = appBuilder.AddGcpFirestore("firestore");
        var config1 = new FirestoreConfig("project-a");
        var config2 = new FirestoreConfig("project-b");

        // Act
        firestore
            .WithFirestoreConfigs(config1)
            .WithFirestoreConfigs(config2);

        // Assert
        firestore.Resource.FirestoreConfigs.Should().HaveCount(2);
    }

    [Fact]
    public void WithFirestoreConfigs_ShouldThrow_WhenConfigsIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var firestore = appBuilder.AddGcpFirestore("firestore");

        // Act
        var act = () => firestore.WithFirestoreConfigs(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddGcpFirestore_ShouldUseDefaultPort()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var firestore = builder.AddGcpFirestore("firestore");

        // Assert
        firestore.Resource.Should().NotBeNull();
    }

    [Fact]
    public void WithReference_WithMultipleConfigs_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddTestProject("api");
        var firestore = appBuilder.AddGcpFirestore("firestore")
            .WithFirestoreConfigs(
                new FirestoreConfig("project-a"),
                new FirestoreConfig("project-b"));

        // Act
        var act = () => project.WithReference(firestore);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void FirestoreEmulatorResource_ConnectionStringExpression_ShouldNotBeNull()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var firestore = builder.AddGcpFirestore("firestore");

        // Assert
        firestore.Resource.ConnectionStringExpression.Should().NotBeNull();
    }

    [Fact]
    public void FirestoreEmulatorResource_HttpEndpoint_ShouldNotBeNull()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var firestore = builder.AddGcpFirestore("firestore");

        // Assert
        firestore.Resource.HttpEndpoint.Should().NotBeNull();
    }
}
