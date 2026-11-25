namespace MVFC.Aspire.Helpers.GcpSpanner.Processors;

/// <summary>
/// Owns the shared provisioning deadline, including endpoint resolution and operation polling.
/// </summary>
internal static class SpannerEmulatorInitializer
{
    /// <summary>
    /// Provisions configured resources and distinguishes host cancellation from deadline expiration.
    /// </summary>
    internal static async Task ConfigureAsync(SpannerEmulatorResource resource, CancellationToken cancellationToken)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(resource.WaitTimeoutSeconds));

        try
        {
            var endpoint = await resource.ConnectionStringExpression.GetValueAsync(deadline.Token).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Spanner emulator endpoint was not resolved.");

            await SpannerConfigProcessor.ConfigureAsync(resource.SpannerConfigs, endpoint, deadline.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested && deadline.IsCancellationRequested)
        {
            throw new TimeoutException($"Spanner provisioning exceeded the configured timeout for resource '{resource.Name}'.", exception);
        }
    }
}
