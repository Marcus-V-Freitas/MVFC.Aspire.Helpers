namespace MVFC.Aspire.Helpers.Playground.Api.Helpers;

/// <summary>
/// Requires an authenticated PING before exposing the playground Redis connection.
/// </summary>
internal static class RedisConnectionInitializer
{
    /// <summary>
    /// Retries bounded startup probes without changing the normal operation settings.
    /// </summary>
    internal static async Task<IConnectionMultiplexer> ConnectAsync(
        ConfigurationOptions options,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        using var deadline = InfrastructureStartup.CreateDeadline(configuration, cancellationToken);
        var started = Stopwatch.GetTimestamp();
        var probeOptions = options.Clone();
        probeOptions.ConnectTimeout = 2000;
        probeOptions.ConnectRetry = 0;
        probeOptions.AbortOnConnectFail = true;
        Exception? lastFailure = null;

        try
        {
            while (true)
            {
                deadline.Token.ThrowIfCancellationRequested();
                ConnectionMultiplexer? connection = null;

                try
                {
                    // ConnectAsync has no cancellation overload; bound and dispose each probe.
                    connection = await ConnectionMultiplexer.ConnectAsync(probeOptions).ConfigureAwait(false);
                    await connection.GetDatabase().PingAsync()
                        .WaitAsync(TimeSpan.FromSeconds(2), deadline.Token).ConfigureAwait(false);
                    deadline.Token.ThrowIfCancellationRequested();

                    var ready = connection;
                    connection = null;
                    return ready;
                }
                catch (RedisConnectionException ex) when (ex.FailureType != ConnectionFailureType.AuthenticationFailure)
                {
                    lastFailure = ex;
                }
                catch (RedisTimeoutException ex)
                {
                    lastFailure = ex;
                }
                catch (TimeoutException ex)
                {
                    lastFailure = ex;
                }
                finally
                {
                    connection?.Dispose();
                }

                await Task.Delay(InfrastructureStartup.RetryDelay, deadline.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && deadline.IsCancellationRequested)
        {
            throw new TimeoutException($"Redis startup connection/PING deadline expired after {Stopwatch.GetElapsedTime(started)}.", lastFailure ?? ex);
        }
    }
}
