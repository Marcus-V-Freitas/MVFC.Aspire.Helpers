namespace MVFC.Aspire.Helpers.GcpPubSub.Extensions;

internal static partial class LogExtensions {

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to configure subscription '{SubscriptionName}'")]
    public static partial void LogSubscriptionConfigFailed(this ILogger logger, Exception exception, string subscriptionName);
}
