namespace MVFC.Aspire.Helpers.GcpSpanner.HealthChecks;

/// <summary>
/// Health check via TCP puro na porta gRPC do emulador.
/// Não envia payload — apenas verifica se a porta está aceitando conexões.
/// </summary>
internal sealed class SpannerGrpcHealthCheck : IHealthCheck
{
    private readonly EndpointReference? _endpoint;
    private readonly int? _port;

    public SpannerGrpcHealthCheck(EndpointReference endpoint)
    {
        _endpoint = endpoint;
    }

    public SpannerGrpcHealthCheck(int port)
    {
        _port = port;
    }

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
