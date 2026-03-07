namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Builds the Pub/Sub project identification string for the PUBSUBn_PROJECT
/// environment variables of the emulator.
/// </summary>
internal static class PubSubProjectBuilder
{
    /// <summary>
    /// Builds the string with projectId, topics, and subscriptions in the format expected by the emulator.
    /// </summary>
    internal static string Build(PubSubConfig pubSubConfig)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pubSubConfig.ProjectId, nameof(pubSubConfig.ProjectId));
        var sb = new StringBuilder(pubSubConfig.ProjectId);

        return !pubSubConfig.MessageConfigs.Any()
            ? sb.ToString()
            : sb.Append(PubSubDefaults.CREATION_DELIMITER)
                 .AppendJoin(PubSubDefaults.CREATION_DELIMITER,
                     pubSubConfig.MessageConfigs.Select(m => BuildTopicSubscription(m) + BuildDeadLetter(m.DeadLetterTopic)))
                 .ToString();
    }

    private static string BuildTopicSubscription(MessageConfig mc) =>
        string.IsNullOrWhiteSpace(mc.SubscriptionName) ? mc.TopicName
        : $"{mc.TopicName}{PubSubDefaults.SUBSCRIPTION_DELIMITER}{mc.SubscriptionName}";

    private static string BuildDeadLetter(string? deadLetterTopic)
    {
        if (string.IsNullOrWhiteSpace(deadLetterTopic))
            return string.Empty;
        var sub = $"{deadLetterTopic}-subscription";
        return $"{PubSubDefaults.CREATION_DELIMITER}{deadLetterTopic}{PubSubDefaults.SUBSCRIPTION_DELIMITER}{sub}";
    }
}
