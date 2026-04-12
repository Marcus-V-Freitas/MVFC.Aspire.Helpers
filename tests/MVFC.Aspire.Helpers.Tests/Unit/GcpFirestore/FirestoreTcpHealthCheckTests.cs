namespace MVFC.Aspire.Helpers.Tests.Unit.GcpFirestore;

using MVFC.Aspire.Helpers.GcpFirestore.HealthChecks;

public sealed class FirestoreTcpHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenUnreachable_ShouldReturnUnhealthy()
    {
        // Arrange
        var healthCheck = new FirestoreTcpHealthCheck(1);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().NotBeNullOrEmpty();
    }
}
