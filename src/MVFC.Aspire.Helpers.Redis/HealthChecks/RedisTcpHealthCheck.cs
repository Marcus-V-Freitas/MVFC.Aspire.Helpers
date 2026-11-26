namespace MVFC.Aspire.Helpers.Redis.HealthChecks;

/// <summary>
/// Verifies connectivity only; authenticated readiness belongs to the consuming client.
/// </summary>
internal sealed class RedisTcpHealthCheck(EndpointReference endpoint) : IHealthCheck
{
    /// <summary>
    /// Connects to the allocated Redis endpoint using the health check cancellation budget.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!endpoint.IsAllocated)
            return HealthCheckResult.Unhealthy("Redis endpoint is not yet allocated.");

        try
        {
            using var connection = new TcpClient();
            await connection.ConnectAsync(endpoint.Host, endpoint.Port, cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (SocketException)
        {
            return HealthCheckResult.Unhealthy("Redis TCP endpoint is not reachable.");
        }
    }
}
