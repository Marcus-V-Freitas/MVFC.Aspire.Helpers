namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Resolves the destination of one push subscription using its consumer's allocated port.
/// </summary>
internal static class PubSubPushEndpointResolver
{
    /// <summary>
    /// Resolves the configured consumer port for a push subscription, or returns an empty address for pull delivery.
    /// </summary>
    internal static async ValueTask<string> ResolveAsync(MessageConfig messageConfig, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(messageConfig.PushEndpoint))
            return string.Empty;

        var endpoint = messageConfig.PushEndpointReference ?? throw new InvalidOperationException($"Pub/Sub push subscription '{messageConfig.SubscriptionName}' requires MessageConfig.PushEndpointReference.");

        await endpoint.GetValueAsync(ct).ConfigureAwait(false);

        return $"http://{PubSubDefaults.DockerInternalHost}:{endpoint.Port.ToString(CultureInfo.InvariantCulture)}";
    }
}
