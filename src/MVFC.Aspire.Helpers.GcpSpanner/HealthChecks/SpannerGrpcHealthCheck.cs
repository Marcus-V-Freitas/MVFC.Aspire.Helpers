namespace MVFC.Aspire.Helpers.GcpSpanner.HealthChecks;

/// <summary>
/// Health check via TCP puro na porta gRPC do emulador.
/// Não envia payload — apenas verifica se a porta está aceitando conexões.
/// </summary>
internal sealed class SpannerGrpcHealthCheck(int port) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync("localhost", port, cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
