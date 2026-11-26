namespace MVFC.Aspire.Helpers.Tests.Unit.Redis;

public sealed class RedisTcpHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenUnallocated_ShouldReturnUnhealthy()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var check = new RedisTcpHealthCheck(builder.AddRedis("redis").Resource.RedisEndpoint);

        // Act
        var result = await check.CheckHealthAsync(new(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenReachable_ShouldReturnHealthy()
    {
        // Arrange
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var check = CreateCheck(((IPEndPoint)listener.LocalEndpoint).Port);

        // Act
        var result = await check.CheckHealthAsync(new(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenPortIsClosed_ShouldReturnUnhealthy()
    {
        // Arrange
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var check = CreateCheck(port);

        // Act
        var result = await check.CheckHealthAsync(new(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCanceled_ShouldPreserveCancellation()
    {
        // Arrange
        var check = CreateCheck(6379);

        // Act
        var act = () => check.CheckHealthAsync(new(), new CancellationToken(true));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Allocates a Redis endpoint against a local test listener.
    /// </summary>
    private static RedisTcpHealthCheck CreateCheck(int port)
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var resource = builder.AddRedis("redis").Resource;
        var endpoint = resource.Annotations.OfType<EndpointAnnotation>().Single();
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "127.0.0.1", port);
        return new RedisTcpHealthCheck(resource.RedisEndpoint);
    }
}
