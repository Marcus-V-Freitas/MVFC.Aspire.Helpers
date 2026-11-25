namespace MVFC.Aspire.Helpers.Tests.Unit.GcpSpanner;

public sealed class SpannerEmulatorInitializerTests
{
    [Fact]
    public async Task ConfigureAsync_WhenEndpointResolutionExceedsDeadline_ShouldThrowTimeout()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var resource = builder.AddGcpSpanner("spanner").WithWaitTimeout(0).Resource;

        // Act
        var act = () => SpannerEmulatorInitializer.ConfigureAsync(resource, TestContext.Current.CancellationToken);

        // Assert
        var error = await act.Should().ThrowAsync<TimeoutException>();
        error.Which.InnerException.Should().BeAssignableTo<OperationCanceledException>();
    }

    [Fact]
    public async Task ConfigureAsync_WhenHostCanceled_ShouldPreserveCancellation()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var resource = builder.AddGcpSpanner("spanner").Resource;
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => SpannerEmulatorInitializer.ConfigureAsync(resource, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
