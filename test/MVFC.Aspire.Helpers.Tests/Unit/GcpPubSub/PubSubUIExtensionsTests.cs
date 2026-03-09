namespace MVFC.Aspire.Helpers.Tests.Utils.GcpPubSub;

public sealed class PubSubUIExtensionsTests
{
    [Fact]
    public void AddGcpPubSubUI_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddGcpPubSubUI("pubsub-ui");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddGcpPubSubUI_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddGcpPubSubUI(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGcpPubSubUI_ShouldSetResourceName()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var ui = builder.AddGcpPubSubUI("pubsub-ui");

        // Assert
        ui.Resource.Name.Should().Be("pubsub-ui");
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<PubSubUIResource>? builder = null;

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
        var ui = appBuilder.AddGcpPubSubUI("pubsub-ui");

        // Act
        var act = () => ui.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenUiBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<PubSubUIResource>? ui = null;
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var emulator = appBuilder.AddGcpPubSub("pubsub");

        // Act
        var act = () => ui!.WithReference(emulator);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithReference_ShouldThrow_WhenEmulatorBuilderIsNull()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var ui = appBuilder.AddGcpPubSubUI("pubsub-ui");
        IResourceBuilder<PubSubEmulatorResource>? emulator = null;

        // Act
        var act = () => ui.WithReference(emulator!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
