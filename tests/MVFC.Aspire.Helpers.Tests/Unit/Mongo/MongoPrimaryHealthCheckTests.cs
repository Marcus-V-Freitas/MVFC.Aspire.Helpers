namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public sealed class MongoPrimaryHealthCheckTests
{
    [Theory]
    [InlineData(true, HealthStatus.Healthy)]
    [InlineData(false, HealthStatus.Unhealthy)]
    [InlineData(null, HealthStatus.Unhealthy)]
    public async Task CheckHealthAsync_ShouldRequireWritablePrimary(bool? writable, HealthStatus expected)
    {
        // Arrange
        var hello = writable.HasValue ? new BsonDocument("isWritablePrimary", writable.Value) : new BsonDocument();
        var check = CreateCheck(_ => Task.FromResult(hello));

        // Act
        var result = await check.CheckHealthAsync(new(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(expected);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenPrimaryFieldHasWrongType_ShouldReturnUnhealthy()
    {
        // Arrange
        var check = CreateCheck(_ => Task.FromResult(new BsonDocument("isWritablePrimary", "true")));

        // Act
        var result = await check.CheckHealthAsync(new(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CheckHealthAsync_WhenUnavailable_ShouldReturnSanitizedFailure(bool driverError)
    {
        // Arrange
        Exception failure = driverError ? new MongoException("sensitive details") : new TimeoutException("sensitive details");
        var check = CreateCheck(_ => Task.FromException<BsonDocument>(failure));

        // Act
        var result = await check.CheckHealthAsync(new(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().NotContain("sensitive");
        result.Exception.Should().BeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenUnallocated_ShouldNotReadHello()
    {
        // Arrange
        var calls = 0;
        var check = CreateCheck(_ =>
        {
            calls++;
            return Task.FromResult(new BsonDocument());
        }, allocated: false);

        // Act
        var result = await check.CheckHealthAsync(new(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCanceledDuringCommand_ShouldPreserveCancellation()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        var check = CreateCheck(ct =>
        {
            cancellation.Cancel();
            return Task.FromCanceled<BsonDocument>(ct);
        });

        // Act
        var act = () => check.CheckHealthAsync(new(), cancellation.Token);

        // Assert
        (await act.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(cancellation.Token);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenAlreadyCanceled_ShouldNotProbe()
    {
        // Arrange
        var check = CreateCheck(_ => throw new InvalidOperationException("Unexpected probe"));

        // Act
        var act = () => check.CheckHealthAsync(new(), new CancellationToken(true));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Creates an allocated endpoint with a controlled MongoDB command response.
    /// </summary>
    private static MongoPrimaryHealthCheck CreateCheck(Func<CancellationToken, Task<BsonDocument>> read, bool allocated = true)
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var resource = builder.AddMongoReplicaSet("mongo").Resource;

        if (allocated)
        {
            var endpoint = resource.Annotations.OfType<EndpointAnnotation>().Single();
            endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 27017);
        }

        return new MongoPrimaryHealthCheck(resource.MongoEndpoint, read);
    }
}
