namespace MVFC.Aspire.Helpers.Tests.Unit.GcpSpanner;

public sealed class SpannerEmulatorExtensionsTests
{
    [Fact]
    public void AddGcpSpanner_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddGcpPubSub("spanner");

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
}
