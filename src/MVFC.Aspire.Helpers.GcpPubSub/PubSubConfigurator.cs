namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Responsible for configuring topics and subscriptions in the Pub/Sub emulator at runtime.
/// </summary>
internal static class PubSubConfigurator
{
    private static readonly Lazy<SubscriberServiceApiClient> _subscriberClient =
        new(() => new SubscriberServiceApiClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
        }.Build());

    private static readonly Lazy<ILogger> _logger =
        new(() => LoggerFactory.Create(b => b.AddConsole()).CreateLogger(nameof(PubSubConfigurator)));

    /// <summary>
    /// Configures all topics and subscriptions for a list of Pub/Sub configs.
    /// </summary>
    internal static async Task ConfigureAsync(
        IReadOnlyList<PubSubConfig> pubSubConfigs,
        int portEndpoint,
        CancellationToken ct)
    {
        foreach (var pubSubConfig in pubSubConfigs)
        {
            var pushEndpoint = $"http://{PubSubDefaults.DockerInternalHost}:{portEndpoint}";
            var tasks = pubSubConfig.MessageConfigs
                .Where(p => !string.IsNullOrWhiteSpace(p.SubscriptionName))
                .Select(mc => ModifyPushEndpoint(pubSubConfig.ProjectId, mc, pushEndpoint, ct))
                .ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Modifies the push endpoint of a Pub/Sub subscription to the correct container address.
    /// </summary>
    private static async Task ModifyPushEndpoint(
        string projectId, MessageConfig messageConfig, string pushEndpoint, CancellationToken ct)
    {
        var subscriptionName = SubscriptionName.FormatProjectSubscription(projectId, messageConfig.SubscriptionName);
        var subscription = await _subscriberClient.Value.GetSubscriptionAsync(subscriptionName).ConfigureAwait(false);
        subscription.PushConfig = BuildPushEndpoint(messageConfig, pushEndpoint);
        subscription.AckDeadlineSeconds = messageConfig.AckDeadlineSeconds ?? PubSubDefaults.ACK_DEADLINE_SECONDS_DEFAULT;
        subscription.DeadLetterPolicy = BuildDeadLetterPolicy(projectId, messageConfig);

        await _subscriberClient.Value.UpdateSubscriptionAsync(subscription, BuildFieldMaskUpdate(messageConfig), ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the dead letter policy for a subscription, if configured.
    /// Returns <see langword="null"/> if no dead letter topic is defined.
    /// </summary>
    private static DeadLetterPolicy? BuildDeadLetterPolicy(string projectId, MessageConfig messageConfig)
    {
        return string.IsNullOrWhiteSpace(messageConfig.DeadLetterTopic)
            ? null
            : new DeadLetterPolicy
        {
            DeadLetterTopic = TopicName.FormatProjectTopic(projectId, messageConfig.DeadLetterTopic),
            MaxDeliveryAttempts = messageConfig.MaxDeliveryAttempts ?? PubSubDefaults.MAX_DELIVERY_ATTEMPTS_DEFAULT
        };
    }

    /// <summary>
    /// Builds the <see cref="FieldMask"/> for the subscription update, including only the fields to change.
    /// </summary>
    private static FieldMask BuildFieldMaskUpdate(MessageConfig messageConfig)
    {
        var paths = new List<string> { "ack_deadline_seconds" };
        if (!string.IsNullOrEmpty(messageConfig.PushEndpoint))
            paths.Add("push_config");
        if (!string.IsNullOrWhiteSpace(messageConfig.DeadLetterTopic))
            paths.Add("dead_letter_policy");
        return new FieldMask { Paths = { paths } };
    }

    /// <summary>
    /// Builds the push configuration for a subscription, combining the base endpoint with the message endpoint suffix.
    /// Returns <see langword="null"/> if no push endpoint is defined.
    /// </summary>
    private static PushConfig? BuildPushEndpoint(MessageConfig messageConfig, string pushEndpoint)
    {
        return string.IsNullOrWhiteSpace(messageConfig.PushEndpoint)
            ? null
            : new PushConfig
        {
            PushEndpoint = $"{pushEndpoint.TrimEnd('/')}/{messageConfig.PushEndpoint.TrimStart('/')}"
        };
    }
}
