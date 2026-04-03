namespace MVFC.Aspire.Helpers.Tests.Unit.GcpSpanner;

public sealed class SpannerEmulatorExtensionsTests
{
    [Fact]
    public void AddGcpSpanner_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddGcpSpanner("spanner");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddGcpSpanner_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGcpSpanner(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGcpSpanner_ShouldThrow_WhenPortIsInvalid()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGcpSpanner("spanner", port: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddGcpSpanner_ShouldSetResourceName()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var spannerBuilder = builder.AddGcpSpanner("spanner");

        // Assert
        spannerBuilder.Resource.Name.Should().Be("spanner");
    }

    [Fact]
    public void WithWaitTimeout_ShouldThrow_WhenSecondsNegative()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var spanner = builder.AddGcpSpanner("spanner");

        // Act
        var act = () => spanner.WithWaitTimeout(-5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    public void WithWaitTimeout_ShouldSetTimeout(int seconds)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var spanner = builder.AddGcpSpanner("spanner");

        // Act
        spanner.WithWaitTimeout(seconds);

        // Assert
        spanner.Resource.WaitTimeoutSeconds.Should().Be(seconds);
    }

    [Fact]
    public void WithSpannerConfigs_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<SpannerEmulatorResource>? builder = null;
        var config = new SpannerConfig("proj", "inst", "db");

        // Act
        var act = () => builder!.WithSpannerConfigs(config);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithSpannerConfigs_ShouldAddConfigs()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var spanner = builder.AddGcpSpanner("spanner");
        var config = new SpannerConfig("proj", "inst", "db");

        // Act
        spanner.WithSpannerConfigs(config);

        // Assert
        spanner.Resource.SpannerConfigs.Should().ContainSingle();
    }

    [Fact]
    public void WithSpannerConfigs_ShouldAccumulateOnChainedCalls()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var spanner = builder.AddGcpSpanner("spanner");
        var config1 = new SpannerConfig("proj1", "inst1", "db1");
        var config2 = new SpannerConfig("proj2", "inst2", "db2");

        // Act
        spanner.WithSpannerConfigs(config1).WithSpannerConfigs(config2);

        // Assert
        spanner.Resource.SpannerConfigs.Should().HaveCount(2);
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<SpannerEmulatorResource>? builder = null;

        // Act
        var act = () => builder!.WithDockerImage("img", "tag");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "tag")]
    [InlineData("", "tag")]
    [InlineData(" ", "tag")]
    [InlineData("img", null)]
    [InlineData("img", "")]
    [InlineData("img", " ")]
    public void WithDockerImage_ShouldThrow_WhenImageOrTagInvalid(string? image, string? tag)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var spanner = appBuilder.AddGcpSpanner("spanner");

        // Act
        var act = () => spanner.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenProjectIsNull()
    {
        // Arrange
        IResourceBuilder<ProjectResource>? project = null;
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var spanner = appBuilder.AddGcpSpanner("spanner");

        // Act
        var act = () => project!.WithReference(spanner);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenSpannerBuilderIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        IResourceBuilder<SpannerEmulatorResource>? spanner = null;

        // Act
        var act = () => project.WithReference(spanner!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_WithoutConfigs_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        var spanner = appBuilder.AddGcpSpanner("spanner");

        // Act
        var act = () => project.WithReference(spanner);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithReference_WithConfigs_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        var spanner = appBuilder.AddGcpSpanner("spanner")
            .WithSpannerConfigs(new SpannerConfig("proj", "inst", "db"));

        // Act
        var act = () => project.WithReference(spanner);

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
        var spanner = appBuilder.AddGcpSpanner("spanner")
            .WithSpannerConfigs(new SpannerConfig("proj", "inst", "db"));

        // Act
        project1.WithReference(spanner);
        project2.WithReference(spanner);

        // Assert
        spanner.Resource.Annotations.OfType<SpannerConfiguredAnnotation>().Should().ContainSingle();
    }

    [Fact]
    public void SpannerConfig_WithNullDdlStatements_ShouldDefaultToEmptyList()
    {
        // Arrange & Act
        var config = new SpannerConfig("proj", "inst", "db", DdlStatements: null);

        // Assert
        config.DdlStatements.Should().BeEmpty();
    }

    [Fact]
    public void SpannerConfig_WithDdlStatements_ShouldRetainStatements()
    {
        // Arrange
        var ddl = new List<string> { "CREATE TABLE Users (Id INT64) PRIMARY KEY (Id)" };

        // Act
        var config = new SpannerConfig("proj", "inst", "db", DdlStatements: ddl);

        // Assert
        config.DdlStatements.Should().ContainSingle();
        config.DdlStatements[0].Should().Contain("CREATE TABLE");
    }
}
