namespace MVFC.Aspire.Helpers.Tests.Unit.Gotenberg;

public sealed class GotenbergExtensionsTests
{
    [Fact]
    public void AddGotenberg_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddGotenberg("gotenberg");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddGotenberg_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGotenberg(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGotenberg_ShouldThrow_WhenPortIsNegative()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGotenberg("gotenberg", port: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddGotenberg_ShouldSetResourceName()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var gb = builder.AddGotenberg("gotenberg");

        // Assert
        gb.Resource.Name.Should().Be("gotenberg");
    }

    [Fact]
    public void AddGotenberg_ConnectionStringExpression_ShouldContainHttpScheme()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var gb = builder.AddGotenberg("gotenberg");

        // Assert
        gb.Resource.ConnectionStringExpression.ValueExpression.Should().Contain("http");
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<GotenbergResource>? builder = null;

        // Act
        var act = () => builder!.WithDockerImage("gotenberg/gotenberg", "8");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "8")]
    [InlineData("", "8")]
    [InlineData(" ", "8")]
    [InlineData("gotenberg/gotenberg", null)]
    [InlineData("gotenberg/gotenberg", "")]
    [InlineData("gotenberg/gotenberg", " ")]
    public void WithDockerImage_ShouldThrow_WhenImageOrTagInvalid(string? image, string? tag)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var gb = appBuilder.AddGotenberg("gotenberg");

        // Act
        var act = () => gb.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenProjectIsNull()
    {
        // Arrange
        IResourceBuilder<ProjectResource>? project = null;
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var gb = appBuilder.AddGotenberg("gotenberg");

        // Act
        var act = () => project!.WithReference(gb);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenGotenbergBuilderIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var project = appBuilder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("api");
        IResourceBuilder<GotenbergResource>? gb = null;

        // Act
        var act = () => project.WithReference(gb!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
