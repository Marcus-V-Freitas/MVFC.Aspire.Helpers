namespace MVFC.Aspire.Helpers.Tests.Unit.GcpSpanner;

public sealed class SpannerGrpcHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenUnreachable_ShouldReturnUnhealthy()
    {
        // Arrange
        var healthCheck = new SpannerGrpcHealthCheck(1); // Usually blocked/unresponsive port

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().NotBeNullOrEmpty();
    }
}
