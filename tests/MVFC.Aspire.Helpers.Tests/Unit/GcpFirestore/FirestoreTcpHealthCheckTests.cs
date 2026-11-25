namespace MVFC.Aspire.Helpers.Tests.Unit.GcpFirestore;

public sealed class FirestoreTcpHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenPortIsReachable_ShouldReturnHealthy()
    {
        // Arrange
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var healthCheck = new FirestoreTcpHealthCheck(port);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenEndpointIsAllocatedAndReachable_ShouldReturnHealthy()
    {
        // Arrange
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var resource = new ContainerResource("firestore");
        var endpoint = new EndpointAnnotation(ProtocolType.Tcp, name: "http");
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "127.0.0.1", port);
        resource.Annotations.Add(endpoint);

        var healthCheck = new FirestoreTcpHealthCheck(new EndpointReference(resource, "http"));

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenEndpointIsNotAllocated_ShouldReturnUnhealthy()
    {
        // Arrange
        var resource = new ContainerResource("firestore");
        resource.Annotations.Add(new EndpointAnnotation(ProtocolType.Tcp, name: "http"));
        var healthCheck = new FirestoreTcpHealthCheck(new EndpointReference(resource, "http"));

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Endpoint is not yet allocated.");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenNoEndpointOrPort_ShouldReturnUnhealthy()
    {
        // Arrange
        var healthCheck = new FirestoreTcpHealthCheck(null!);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("No port or endpoint configured.");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCancelled_ShouldReturnUnhealthy()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var healthCheck = new FirestoreTcpHealthCheck(1);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), cancellation.Token);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().NotBeNullOrEmpty();
    }

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
