namespace MVFC.Aspire.Helpers.Tests.Unit.GcpBigQuery;

public sealed class BigQueryEmulatorExtensionsTests
{
    [Fact]
    public void AddGcpBigQuery_ShouldThrow_WhenBuilderIsNull()
    {
        IDistributedApplicationBuilder? builder = null;
        var act = () => builder!.AddGcpBigQuery("bigquery");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddGcpBigQuery_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var act = () => builder.AddGcpBigQuery(name!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGcpBigQuery_ShouldThrow_WhenRestPortIsInvalid()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var act = () => builder.AddGcpBigQuery("bigquery", restPort: -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddGcpBigQuery_ShouldThrow_WhenGrpcPortIsInvalid()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var act = () => builder.AddGcpBigQuery("bigquery", grpcPort: -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddGcpBigQuery_ShouldSetResourceName()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var bigquery = builder.AddGcpBigQuery("bigquery");
        bigquery.Resource.Name.Should().Be("bigquery");
    }

    [Fact]
    public void AddGcpBigQuery_ShouldInitializeWithEmptyConfigs()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var bigquery = builder.AddGcpBigQuery("bigquery");
        bigquery.Resource.BigQueryConfigs.Should().BeEmpty();
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        IResourceBuilder<BigQueryEmulatorResource>? builder = null;
        var act = () => builder!.WithDockerImage("img", "tag");
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
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");

        var act = () => bigquery.WithDockerImage(image!, tag!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithBigQueryConfigs_ShouldThrow_WhenBuilderIsNull()
    {
        IResourceBuilder<BigQueryEmulatorResource>? builder = null;
        var config = new BigQueryConfig("my-project");

        var act = () => builder!.WithBigQueryConfigs(config);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithBigQueryConfigs_ShouldAddSingleConfig()
    {
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");
        var config = new BigQueryConfig("project-a");

        bigquery.WithBigQueryConfigs(config);

        bigquery.Resource.BigQueryConfigs.Should().HaveCount(1);
        bigquery.Resource.BigQueryConfigs[0].ProjectId.Should().Be("project-a");
    }

    [Fact]
    public void WithBigQueryConfigs_ShouldAddMultipleConfigs()
    {
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");
        var config1 = new BigQueryConfig("project-a");
        var config2 = new BigQueryConfig("project-b", "test_dataset");

        bigquery.WithBigQueryConfigs(config1, config2);

        bigquery.Resource.BigQueryConfigs.Should().HaveCount(2);
        bigquery.Resource.BigQueryConfigs.Select(c => c.ProjectId)
            .Should().BeEquivalentTo(["project-a", "project-b"]);
    }

    [Fact]
    public void WithBigQueryConfigs_ShouldThrow_WhenConfigsIsNull()
    {
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");

        var act = () => bigquery.WithBigQueryConfigs(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void WithDataSeed_ShouldThrow_WhenBuilderIsNull()
    {
        IResourceBuilder<BigQueryEmulatorResource>? builder = null;
        var act = () => builder!.WithDataSeed("path");
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WithDataSeed_ShouldThrow_WhenPathIsInvalid(string? path)
    {
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");

        var act = () => bigquery.WithDataSeed(path!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenProjectIsNull()
    {
        IResourceBuilder<ProjectResource>? project = null;
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");

        var act = () => project!.WithReference(bigquery);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenBigQueryBuilderIsNull()
    {
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        IResourceBuilder<BigQueryEmulatorResource>? bigquery = null;

        var act = () => project.WithReference(bigquery!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldNotThrow()
    {
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        var bigquery = appBuilder.AddGcpBigQuery("bigquery");

        var act = () => project.WithReference(bigquery);

        act.Should().NotThrow();
    }

    [Fact]
    public void BigQueryEmulatorResource_ConnectionStringExpression_ShouldNotBeNull()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var bigquery = builder.AddGcpBigQuery("bigquery");

        bigquery.Resource.ConnectionStringExpression.Should().NotBeNull();
    }

    [Fact]
    public void BigQueryEmulatorResource_Endpoints_ShouldNotBeNull()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var bigquery = builder.AddGcpBigQuery("bigquery");

        bigquery.Resource.RestEndpoint.Should().NotBeNull();
        bigquery.Resource.GrpcEndpoint.Should().NotBeNull();
    }
}
