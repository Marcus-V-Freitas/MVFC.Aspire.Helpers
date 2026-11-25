namespace MVFC.Aspire.Helpers.GcpPubSub.Extensions;

/// <summary>
/// Structured logging messages for Pub/Sub subscription initialization.
/// </summary>
internal static partial class LogExtensions
{
    /// <summary>
    /// Logs the number of subscriptions being configured.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Configuring {Count} Pub/Sub subscriptions")]
    public static partial void LogConfigurationStarting(this ILogger logger, int count);

    /// <summary>
    /// Logs the configured subscription count and elapsed duration.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Configured {Count} Pub/Sub subscriptions in {ElapsedMilliseconds} ms")]
    public static partial void LogConfigurationCompleted(this ILogger logger, int count, double elapsedMilliseconds);

    /// <summary>
    /// Logs the project, subscription and status of a final configuration failure.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to configure Pub/Sub subscription '{Subscription}' in project '{ProjectId}' ({StatusCode})")]
    public static partial void LogSubscriptionConfigurationFailed(this ILogger logger, string projectId, string subscription, StatusCode statusCode, Exception exception);

    /// <summary>
    /// Logs expiration of the shared subscription initialization deadline.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Pub/Sub subscription configuration timed out after {TimeoutSeconds} seconds")]
    public static partial void LogConfigurationTimedOut(this ILogger logger, int timeoutSeconds);
}
