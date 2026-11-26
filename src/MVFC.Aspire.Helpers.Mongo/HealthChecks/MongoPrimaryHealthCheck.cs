namespace MVFC.Aspire.Helpers.Mongo.HealthChecks;

/// <summary>
/// Checks for a writable MongoDB primary using a client reused across probes.
/// </summary>
internal sealed class MongoPrimaryHealthCheck : IHealthCheck
{
    /// <summary>
    /// Deferred endpoint allocated by Aspire before probing.
    /// </summary>
    private readonly EndpointReference _endpoint;

    /// <summary>
    /// Executes hello with the probe cancellation budget.
    /// </summary>
    private readonly Func<CancellationToken, Task<BsonDocument>> _readHello;

    /// <summary>
    /// Creates a probe whose MongoDB client is initialized only after endpoint allocation.
    /// </summary>
    internal MongoPrimaryHealthCheck(
        EndpointReference endpoint,
        Func<CancellationToken, Task<BsonDocument>>? readHello = null)
    {
        _endpoint = endpoint;
        var client = new Lazy<IMongoClient>(() => new MongoClient(new MongoClientSettings
        {
            Server = new MongoServerAddress(endpoint.Host, endpoint.Port),
            DirectConnection = true,
            ConnectTimeout = TimeSpan.FromSeconds(2),
            ServerSelectionTimeout = TimeSpan.FromSeconds(2),
        }));

        _readHello = readHello ?? (ct => client.Value.GetDatabase("admin")
            .RunCommandAsync<BsonDocument>(new BsonDocument("hello", 1), cancellationToken: ct));
    }

    /// <summary>
    /// Reports election or connectivity failures without exposing connection details.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_endpoint.IsAllocated)
            return HealthCheckResult.Unhealthy("MongoDB endpoint is not yet allocated.");

        try
        {
            var hello = await _readHello(cancellationToken).ConfigureAwait(false);

            return hello.TryGetValue("isWritablePrimary", out var primary) && primary.IsBoolean && primary.AsBoolean
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("MongoDB has not elected a writable primary.");
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException)
        {
            return HealthCheckResult.Unhealthy("MongoDB primary is not reachable.");
        }
    }
}
