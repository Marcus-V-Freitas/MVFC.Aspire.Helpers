namespace MVFC.Aspire.Helpers.Playground.Api.Helpers;

/// <summary>
/// Opens the playground AMQP connection under a bounded startup deadline.
/// </summary>
internal static class RabbitConnectionInitializer
{
    /// <summary>
    /// Retries transport failures while preserving authentication errors and caller cancellation.
    /// </summary>
    internal static async Task<IConnection> ConnectAsync(
        ConnectionFactory factory,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        using var deadline = InfrastructureStartup.CreateDeadline(configuration, cancellationToken);
        var started = Stopwatch.GetTimestamp();
        Exception? lastFailure = null;

        try
        {
            while (true)
            {
                deadline.Token.ThrowIfCancellationRequested();

                try
                {
                    return await factory.CreateConnectionAsync(deadline.Token).ConfigureAwait(false);
                }
                catch (BrokerUnreachableException ex) when (IsTransportFailure(ex.InnerException))
                {
                    lastFailure = ex;
                }

                await Task.Delay(InfrastructureStartup.RetryDelay, deadline.Token).ConfigureAwait(false);
            }
        }
        catch (BrokerUnreachableException ex) when (deadline.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException($"RabbitMQ startup AMQP connection deadline expired after {Stopwatch.GetElapsedTime(started)}.", ex);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && deadline.IsCancellationRequested)
        {
            throw new TimeoutException($"RabbitMQ startup AMQP connection deadline expired after {Stopwatch.GetElapsedTime(started)}.", lastFailure ?? ex);
        }
    }

    /// <summary>
    /// Retries only known transport errors, including SDK aggregate endpoint failures.
    /// </summary>
    private static bool IsTransportFailure(Exception? exception) => exception switch
    {
        ConnectFailureException failure => IsTransportFailure(failure.InnerException),
        SocketException or IOException or TimeoutException => true,
        AggregateException aggregate => aggregate.InnerExceptions.Count > 0 && aggregate.InnerExceptions.All(IsTransportFailure),
        _ => false,
    };
}
