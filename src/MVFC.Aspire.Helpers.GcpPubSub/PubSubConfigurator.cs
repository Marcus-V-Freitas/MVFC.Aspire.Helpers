namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Responsible for configuring topics and subscriptions in the Pub/Sub emulator at runtime.
/// </summary>
internal static class PubSubConfigurator
{
    /// <summary>
    /// Configures all topics and subscriptions for a list of Pub/Sub configs.
    /// </summary>
    internal static Task ConfigureAsync(
        IReadOnlyList<PubSubConfig> pubSubConfigs,
        string endpoint,
        CancellationToken ct,
        ILogger? logger = null) =>
        ConfigureAsync(pubSubConfigs, new SubscriberServiceApiClientBuilder
        {
            ChannelCredentials = ChannelCredentials.Insecure,
            Endpoint = endpoint,
            Settings = CreateSubscriberSettings(),
        }, ct, logger);

    /// <summary>
    /// Configures subscriptions within a shared deadline, including client creation and SDK retries.
    /// </summary>
    internal static async Task ConfigureAsync(
        IReadOnlyList<PubSubConfig> pubSubConfigs,
        SubscriberServiceApiClientBuilder clientBuilder,
        CancellationToken ct,
        ILogger? logger = null,
        TimeProvider? timeProvider = null)
    {
        ct.ThrowIfCancellationRequested();
        var subscriptions = PubSubConfigurationValidator.GetSubscriptions(pubSubConfigs);

        if (subscriptions.Count == 0)
            return;

        logger ??= NullLogger.Instance;
        timeProvider ??= TimeProvider.System;

        await PubSubConfigurationTimeout.RunAsync(
            token => ConfigureSubscriptionsAsync(subscriptions, clientBuilder, logger, timeProvider, token),
            logger, timeProvider, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether any message configuration declares a subscription.
    /// </summary>
    internal static bool HasSubscriptions(IReadOnlyList<PubSubConfig> pubSubConfigs) =>
        pubSubConfigs.Any(c => c.MessageConfigs.Any(m => !string.IsNullOrWhiteSpace(m.SubscriptionName)));

    /// <summary>
    /// Creates an emulator client and configures subscriptions using the supplied cancellation budget.
    /// </summary>
    internal static Task ConfigureSubscriptionsAsync(
        IReadOnlyList<PubSubConfig> pubSubConfigs,
        string endpoint,
        ILogger logger,
        TimeProvider timeProvider,
        CancellationToken ct) =>
        ConfigureSubscriptionsAsync(PubSubConfigurationValidator.GetSubscriptions(pubSubConfigs), new SubscriberServiceApiClientBuilder
        {
            ChannelCredentials = ChannelCredentials.Insecure,
            Endpoint = endpoint,
            Settings = CreateSubscriberSettings(),
        }, logger, timeProvider, ct);

    /// <summary>
    /// Configures all declared subscriptions concurrently and logs the completion duration.
    /// </summary>
    private static async Task ConfigureSubscriptionsAsync(
        List<(string ProjectId, MessageConfig MessageConfig)> subscriptions,
        SubscriberServiceApiClientBuilder clientBuilder,
        ILogger logger,
        TimeProvider timeProvider,
        CancellationToken ct)
    {
        var started = timeProvider.GetTimestamp();
        logger.LogConfigurationStarting(subscriptions.Count);
        ct.ThrowIfCancellationRequested();

        var client = await clientBuilder.BuildAsync(ct).ConfigureAwait(false);
        var tasks = subscriptions.ConvertAll(s => ConfigureSubscriptionAsync(
            client, s.ProjectId, s.MessageConfig, logger, ct));

        await Task.WhenAll(tasks).ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogConfigurationCompleted(subscriptions.Count, timeProvider.GetElapsedTime(started).TotalMilliseconds);
    }

    /// <summary>
    /// Resolves one subscription destination, updates its settings and logs final RPC failures.
    /// </summary>
    private static async Task ConfigureSubscriptionAsync(
        SubscriberServiceApiClient client,
        string projectId,
        MessageConfig messageConfig,
        ILogger logger,
        CancellationToken ct)
    {
        try
        {
            var pushEndpoint = await PubSubPushEndpointResolver.ResolveAsync(messageConfig, ct).ConfigureAwait(false);

            await ModifyPushEndpoint(client, projectId, messageConfig, pushEndpoint, ct).ConfigureAwait(false);
        }
        catch (RpcException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogSubscriptionConfigurationFailed(projectId, messageConfig.SubscriptionName!, ex.StatusCode, ex);
            throw;
        }
    }

    /// <summary>
    /// Adds a bounded startup window for subscriptions created by the emulator image.
    /// Other read failures retain the SDK's transient-error classification; updates retain SDK defaults.
    /// </summary>
    internal static SubscriberServiceApiSettings CreateSubscriberSettings()
    {
        var settings = SubscriberServiceApiSettings.GetDefault();
        var retryFilter = settings.GetSubscriptionSettings.Retry.RetryFilter;
        settings.GetSubscriptionSettings = settings.GetSubscriptionSettings.WithRetry(
            RetrySettings.FromExponentialBackoff(
                maxAttempts: PubSubDefaults.CONFIGURATION_MAX_ATTEMPTS,
                initialBackoff: TimeSpan.FromMilliseconds(200),
                maxBackoff: TimeSpan.FromSeconds(2),
                backoffMultiplier: 1.5,
                retryFilter: ex => ex is RpcException { StatusCode: StatusCode.NotFound } || retryFilter(ex)));
        return settings;
    }

    /// <summary>
    /// Modifies the push endpoint of a Pub/Sub subscription to the correct container address.
    /// </summary>
    internal static async Task ModifyPushEndpoint(
        string projectId, MessageConfig messageConfig, string pushEndpoint, CancellationToken ct)
    {
        var client = await new SubscriberServiceApiClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
        }.BuildAsync(ct).ConfigureAwait(false);

        await ModifyPushEndpoint(client, projectId, messageConfig, pushEndpoint, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Modifies the push endpoint of a Pub/Sub subscription using the provided client instance.
    /// </summary>
    internal static async Task ModifyPushEndpoint(
        SubscriberServiceApiClient client,
        string projectId,
        MessageConfig messageConfig,
        string pushEndpoint,
        CancellationToken ct)
    {
        var subscriptionName = SubscriptionName.FormatProjectSubscription(projectId, messageConfig.SubscriptionName);
        var subscription = await client.GetSubscriptionAsync(subscriptionName, ct).ConfigureAwait(false);
        subscription.PushConfig = BuildPushEndpoint(messageConfig, pushEndpoint);
        subscription.AckDeadlineSeconds = messageConfig.AckDeadlineSeconds ?? PubSubDefaults.ACK_DEADLINE_SECONDS_DEFAULT;
        subscription.DeadLetterPolicy = BuildDeadLetterPolicy(projectId, messageConfig);

        await client.UpdateSubscriptionAsync(subscription, BuildFieldMaskUpdate(messageConfig), ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the dead letter policy for a subscription, if configured.
    /// Returns <see langword="null"/> if no dead letter topic is defined.
    /// </summary>
    internal static DeadLetterPolicy? BuildDeadLetterPolicy(string projectId, MessageConfig messageConfig)
    {
        return string.IsNullOrWhiteSpace(messageConfig.DeadLetterTopic)
            ? null
            : new DeadLetterPolicy
        {
            DeadLetterTopic = TopicName.FormatProjectTopic(projectId, messageConfig.DeadLetterTopic),
            MaxDeliveryAttempts = messageConfig.MaxDeliveryAttempts ?? PubSubDefaults.MAX_DELIVERY_ATTEMPTS_DEFAULT,
        };
    }

    /// <summary>
    /// Builds the <see cref="FieldMask"/> for the subscription update, including only the fields to change.
    /// </summary>
    internal static FieldMask BuildFieldMaskUpdate(MessageConfig messageConfig)
    {
        var paths = new List<string> { "ack_deadline_seconds" };
        if (!string.IsNullOrWhiteSpace(messageConfig.PushEndpoint))
            paths.Add("push_config");
        if (!string.IsNullOrWhiteSpace(messageConfig.DeadLetterTopic))
            paths.Add("dead_letter_policy");
        return new FieldMask { Paths = { paths } };
    }

    /// <summary>
    /// Builds the push configuration for a subscription, combining the base endpoint with the message endpoint suffix.
    /// Returns <see langword="null"/> if no push endpoint is defined.
    /// </summary>
    internal static PushConfig? BuildPushEndpoint(MessageConfig messageConfig, string pushEndpoint)
    {
        return string.IsNullOrWhiteSpace(messageConfig.PushEndpoint)
            ? null
            : new PushConfig
        {
            PushEndpoint = $"{pushEndpoint.TrimEnd('/')}/{messageConfig.PushEndpoint.TrimStart('/')}",
        };
    }
}
