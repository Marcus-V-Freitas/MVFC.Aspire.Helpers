namespace MVFC.Aspire.Helpers.GcpFirestore.HealthChecks;

/// <summary>
/// Health check using raw TCP on the emulator's HTTP port.
/// The Firestore emulator does not expose a reliable HTTP health check endpoint —
/// this only verifies if the port is accepting connections.
/// </summary>
/// <param name="port">The port to check for TCP connectivity.</param>
internal sealed class FirestoreTcpHealthCheck : IHealthCheck
{
    private readonly EndpointReference? _endpoint;
    private readonly int? _port;

    public FirestoreTcpHealthCheck(EndpointReference endpoint)
    {
        _endpoint = endpoint;
    }

    public FirestoreTcpHealthCheck(int port)
    {
        _port = port;
    }

    /// <summary>
    /// Checks the health of the Firestore emulator by attempting a TCP connection to the specified port.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while performing the health check.</param>
    /// <returns>A <see cref="HealthCheckResult"/> indicating whether the emulator is reachable via TCP.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var host = "localhost";
            int port;

            if (_endpoint is not null)
            {
                if (!_endpoint.IsAllocated)
                {
                    return HealthCheckResult.Unhealthy("Endpoint is not yet allocated.");
                }

                host = _endpoint.Host;
                port = _endpoint.Port;
            }
            else if (_port.HasValue)
            {
                port = _port.Value;
            }
            else
            {
                return HealthCheckResult.Unhealthy("No port or endpoint configured.");
            }

            using var tcp = new TcpClient();
            await tcp.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}