namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Resolves runtime endpoints and configures subscriptions under a single deadline.
/// </summary>
internal static class PubSubEmulatorInitializer
{
    /// <summary>
    /// Resolves the emulator endpoint and configures subscriptions within the initialization deadline.
    /// </summary>
    internal static Task InitializeAsync(
        PubSubEmulatorResource resource,
        ILogger logger,
        CancellationToken ct,
        TimeProvider? timeProvider = null)
    {
        if (!PubSubConfigurator.HasSubscriptions(resource.PubSubConfigs))
            return Task.CompletedTask;

        timeProvider ??= TimeProvider.System;

        return PubSubConfigurationTimeout.RunAsync(async token =>
        {
            var connectionString = await resource.ConnectionStringExpression.GetValueAsync(token).ConfigureAwait(false) ?? throw new InvalidOperationException("Pub/Sub emulator endpoint was not resolved.");

            await PubSubConfigurator.ConfigureSubscriptionsAsync(resource.PubSubConfigs, connectionString, logger, timeProvider, token).ConfigureAwait(false);
        }, logger, timeProvider, ct, resource.ConfigurationTimeoutSeconds);
    }
}
