namespace MVFC.Aspire.Helpers.Playground.Api.Helpers;

/// <summary>
/// Holds the playground infrastructure startup deadline and retry interval.
/// </summary>
internal static class InfrastructureStartup
{
    /// <summary>
    /// Configuration key for each client startup deadline, in seconds.
    /// </summary>
    internal const string TimeoutConfigurationKey = "Infrastructure:StartupTimeoutSeconds";

    /// <summary>
    /// Interval between failed connection probes.
    /// </summary>
    internal static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Creates a shared deadline for one client initialization operation.
    /// </summary>
    internal static CancellationTokenSource CreateDeadline(IConfiguration configuration, CancellationToken cancellationToken)
    {
        var seconds = configuration.GetValue(TimeoutConfigurationKey, 30);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seconds);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(seconds, 4_294_967);

        var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(seconds));
        return deadline;
    }
}
