namespace MVFC.Aspire.Helpers.Tests.Unit.GcpSpanner;

public sealed class SpannerGrpcHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenEndpointIsNotAllocated_ShouldReturnUnhealthyWithoutConnecting()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var spanner = builder.AddGcpSpanner("spanner");
        var healthCheck = new SpannerGrpcHealthCheck(spanner.GetEndpoint("grpc"));

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Endpoint is not yet allocated.");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenEndpointIsMissing_ShouldExplainUnhealthyConfiguration()
    {
        // Arrange
        var healthCheck = new SpannerGrpcHealthCheck((EndpointReference)null!);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("No port or endpoint configured.");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenAllocatedEndpointAcceptsConnections_ShouldReturnHealthy()
    {
        // Arrange
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var builder = DistributedApplication.CreateBuilder([]);
        var resource = builder.AddContainer("spanner", "image").WithEndpoint(name: "grpc", targetPort: 9010);
        var endpoint = resource.Resource.Annotations.OfType<EndpointAnnotation>().Single();
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "127.0.0.1", ((IPEndPoint)listener.LocalEndpoint).Port);
        var healthCheck = new SpannerGrpcHealthCheck(resource.GetEndpoint("grpc"));

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Exception.Should().BeNull();
    }

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
