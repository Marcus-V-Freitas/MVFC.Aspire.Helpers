namespace MVFC.Aspire.Helpers.Tests.Unit.GcpBigQuery;

public sealed class BigQueryEmulatorExtensionsTests
{
    [Fact]
    public void AddGcpBigQuery_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddGcpBigQuery("bigquery");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddGcpBigQuery_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGcpBigQuery(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGcpBigQuery_ShouldThrow_WhenRestPortIsInvalid()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGcpBigQuery("bigquery", restPort: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddGcpBigQuery_ShouldThrow_WhenGrpcPortIsInvalid()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGcpBigQuery("bigquery", grpcPort: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddGcpBigQuery_ShouldSetResourceName()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var bigquery = builder.AddGcpBigQuery("bigquery");

        // Assert
        bigquery.Resource.Name.Should().Be("bigquery");
    }

    [Fact]
    public void AddGcpBigQuery_ShouldInitializeWithEmptyConfigs()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var bigquery = builder.AddGcpBigQuery("bigquery");

        // Assert
        bigquery.Resource.BigQueryConfigs.Should().BeEmpty();
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<BigQueryEmulatorResource>? builder = null;

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
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");

        // Act
        var act = () => bigquery.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithBigQueryConfigs_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<BigQueryEmulatorResource>? builder = null;
        var config = new BigQueryConfig("my-project");

        // Act
        var act = () => builder!.WithBigQueryConfigs(config);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithBigQueryConfigs_ShouldAddSingleConfig()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");
        var config = new BigQueryConfig("project-a");

        // Act
        bigquery.WithBigQueryConfigs(config);

        // Assert
        bigquery.Resource.BigQueryConfigs.Should().HaveCount(1);
        bigquery.Resource.BigQueryConfigs[0].ProjectId.Should().Be("project-a");
    }

    [Fact]
    public void WithBigQueryConfigs_ShouldAddMultipleConfigs()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");
        var config1 = new BigQueryConfig("project-a");
        var config2 = new BigQueryConfig("project-b", "test_dataset");

        // Act
        bigquery.WithBigQueryConfigs(config1, config2);

        // Assert
        bigquery.Resource.BigQueryConfigs.Should().HaveCount(2);
        bigquery.Resource.BigQueryConfigs.Select(c => c.ProjectId)
            .Should().BeEquivalentTo(["project-a", "project-b"]);
    }

    [Fact]
    public void WithBigQueryConfigs_ShouldThrow_WhenConfigsIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");

        // Act
        var act = () => bigquery.WithBigQueryConfigs(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void WithDataSeed_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<BigQueryEmulatorResource>? builder = null;

        // Act
        var act = () => builder!.WithDataSeed("path");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WithDataSeed_ShouldThrow_WhenPathIsInvalid(string? path)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");

        // Act
        var act = () => bigquery.WithDataSeed(path!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenProjectIsNull()
    {
        // Arrange
        IResourceBuilder<ProjectResource>? project = null;
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");

        // Act
        var act = () => project!.WithReference(bigquery);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenBigQueryBuilderIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddTestProject("api");
        IResourceBuilder<BigQueryEmulatorResource>? bigquery = null;

        // Act
        var act = () => project.WithReference(bigquery!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddTestProject("api");
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");

        // Act
        var act = () => project.WithReference(bigquery);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void BigQueryEmulatorResource_ConnectionStringExpression_ShouldNotBeNull()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var bigquery = builder.AddGcpBigQuery("bigquery");

        // Assert
        bigquery.Resource.ConnectionStringExpression.Should().NotBeNull();
    }

    [Fact]
    public void BigQueryEmulatorResource_Endpoints_ShouldNotBeNull()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var bigquery = builder.AddGcpBigQuery("bigquery");

        // Assert
        bigquery.Resource.RestEndpoint.Should().NotBeNull();
        bigquery.Resource.GrpcEndpoint.Should().NotBeNull();
    }
}
