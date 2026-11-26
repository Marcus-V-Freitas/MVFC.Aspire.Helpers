namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Applies one deadline to the entire subscription initialization operation.
/// </summary>
internal static class PubSubConfigurationTimeout
{
    /// <summary>
    /// Runs initialization under a shared deadline while preserving caller cancellation.
    /// </summary>
    internal static async Task RunAsync(
        Func<CancellationToken, Task> configure,
        ILogger logger,
        TimeProvider timeProvider,
        CancellationToken ct,
        int timeoutSeconds = PubSubDefaults.CONFIGURATION_TIMEOUT_SECONDS)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeoutSeconds);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(timeoutSeconds, PubSubDefaults.MAX_CONFIGURATION_TIMEOUT_SECONDS);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds), timeProvider);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);

        try
        {
            cancellation.Token.ThrowIfCancellationRequested();

            await configure(cancellation.Token).ConfigureAwait(false);

            cancellation.Token.ThrowIfCancellationRequested();
        }
        catch (Exception ex) when (
            (ex is OperationCanceledException or RpcException { StatusCode: StatusCode.Cancelled or StatusCode.DeadlineExceeded })
            && cancellation.IsCancellationRequested)
        {
            ct.ThrowIfCancellationRequested();
            logger.LogConfigurationTimedOut(timeoutSeconds);

            throw new TimeoutException($"Pub/Sub subscription configuration did not complete within {timeoutSeconds} seconds.", ex);
        }
    }
}
