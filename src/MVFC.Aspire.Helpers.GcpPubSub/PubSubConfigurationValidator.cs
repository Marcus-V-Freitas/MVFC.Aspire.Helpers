namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Validates static configuration before endpoint resolution or emulator RPCs.
/// </summary>
internal static class PubSubConfigurationValidator
{
    /// <summary>
    /// Returns one configuration per subscription and rejects conflicting declarations.
    /// Identical effective declarations are coalesced without resolving endpoint allocations.
    /// </summary>
    internal static List<(string ProjectId, MessageConfig MessageConfig)> GetSubscriptions(IReadOnlyList<PubSubConfig> configs)
    {
        ArgumentNullException.ThrowIfNull(configs);

        var subscriptions = new Dictionary<(string ProjectId, string Subscription), MessageConfig>();

        foreach (var config in configs)
        {
            if (config is null)
                throw new ArgumentException("Project configuration must not be null.", nameof(configs));

            if (string.IsNullOrWhiteSpace(config.ProjectId))
                throw new ArgumentException("ProjectId must not be blank.", nameof(configs));

            if (config.MessageConfigs is null)
                throw new ArgumentException("MessageConfigs must not be null.", nameof(configs));

            foreach (var message in config.MessageConfigs)
            {
                if (message is null)
                    throw new ArgumentException("Message configuration must not be null.", nameof(configs));

                if (string.IsNullOrWhiteSpace(message.TopicName))
                    throw new ArgumentException("TopicName must not be blank.", nameof(configs));

                ValidateSubscription(message);

                if (string.IsNullOrWhiteSpace(message.SubscriptionName))
                    continue;

                var key = (config.ProjectId, message.SubscriptionName);

                if (subscriptions.TryGetValue(key, out var existing) && !AreEquivalent(existing, message))
                    throw new InvalidOperationException($"Pub/Sub subscription '{message.SubscriptionName}' in project '{config.ProjectId}' has conflicting configurations.");

                subscriptions.TryAdd(key, message);
            }
        }

        return subscriptions.Select(pair => (pair.Key.ProjectId, pair.Value)).ToList();
    }

    /// <summary>
    /// Validates push requirements and subscription policy ranges without network access.
    /// Zero retains the service defaults for explicitly configured acknowledgment and delivery values.
    /// </summary>
    private static void ValidateSubscription(MessageConfig message)
    {
        if (!string.IsNullOrWhiteSpace(message.PushEndpoint))
        {
            if (string.IsNullOrWhiteSpace(message.SubscriptionName))
                throw new InvalidOperationException("Pub/Sub push delivery requires a SubscriptionName.");

            if (message.PushEndpointReference is null)
                throw new InvalidOperationException($"Pub/Sub push subscription '{message.SubscriptionName}' requires MessageConfig.PushEndpointReference.");
        }

        if (string.IsNullOrWhiteSpace(message.SubscriptionName))
            return;

        if (message.AckDeadlineSeconds is { } deadline && deadline != 0 &&
            (deadline < PubSubDefaults.ACK_DEADLINE_SECONDS_MIN || deadline > PubSubDefaults.ACK_DEADLINE_SECONDS_MAX))
            throw new ArgumentOutOfRangeException(nameof(message), deadline, "AckDeadlineSeconds must be 0 (service default), or between 10 and 600 seconds.");

        if (!string.IsNullOrWhiteSpace(message.DeadLetterTopic) &&
            message.MaxDeliveryAttempts is { } attempts && attempts != 0 &&
            (attempts < PubSubDefaults.MAX_DELIVERY_ATTEMPTS_MIN || attempts > PubSubDefaults.MAX_DELIVERY_ATTEMPTS_MAX))
            throw new ArgumentOutOfRangeException(nameof(message), attempts, "MaxDeliveryAttempts must be 0 (service default), or between 5 and 100.");
    }

    /// <summary>
    /// Compares the fields applied to a subscription, including effective defaults and push destination identity.
    /// </summary>
    private static bool AreEquivalent(MessageConfig first, MessageConfig second) =>
        first.TopicName == second.TopicName &&
        EffectiveAckDeadline(first) == EffectiveAckDeadline(second) &&
        Normalize(first.DeadLetterTopic) == Normalize(second.DeadLetterTopic) &&
        (string.IsNullOrWhiteSpace(first.DeadLetterTopic) || EffectiveDeliveryAttempts(first) == EffectiveDeliveryAttempts(second)) &&
        Normalize(first.PushEndpoint).TrimStart('/') == Normalize(second.PushEndpoint).TrimStart('/') &&
        (string.IsNullOrWhiteSpace(first.PushEndpoint) || SameDestination(first.PushEndpointReference!, second.PushEndpointReference!));

    /// <summary>
    /// Compares resource and endpoint identity without waiting for an allocated port.
    /// </summary>
    private static bool SameDestination(EndpointReference first, EndpointReference second) =>
        ReferenceEquals(first.Resource, second.Resource) && first.EndpointName == second.EndpointName;

    /// <summary>
    /// Normalizes absent optional fields using the same whitespace semantics as configuration updates.
    /// </summary>
    private static string Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value;

    /// <summary>
    /// Distinguishes the library default from the service's explicit zero default.
    /// </summary>
    private static int EffectiveAckDeadline(MessageConfig message) =>
        message.AckDeadlineSeconds == 0 ? PubSubDefaults.ACK_DEADLINE_SECONDS_MIN : message.AckDeadlineSeconds ?? PubSubDefaults.ACK_DEADLINE_SECONDS_DEFAULT;

    /// <summary>
    /// Resolves the service and library dead letter delivery defaults.
    /// </summary>
    private static int EffectiveDeliveryAttempts(MessageConfig message) =>
        message.MaxDeliveryAttempts is null or 0 ? PubSubDefaults.MAX_DELIVERY_ATTEMPTS_DEFAULT : message.MaxDeliveryAttempts.Value;
}
