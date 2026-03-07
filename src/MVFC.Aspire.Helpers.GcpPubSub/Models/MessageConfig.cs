namespace MVFC.Aspire.Helpers.GcpPubSub.Models;

/// <summary>
/// Represents the message configuration for Pub/Sub, including the topic name, subscription name,
/// push endpoint for message delivery, dead letter settings, and the acknowledgment deadline.
/// </summary>
/// <param name="TopicName">
/// Pub/Sub topic name to which the message will be published.
/// </param>
/// <param name="SubscriptionName">
/// (Optional) Subscription name associated with the topic.
/// </param>
/// <param name="PushEndpoint">
/// (Optional) HTTP endpoint for push message delivery. If not provided, the subscription will be pull-based.
/// </param>
public sealed record class MessageConfig(
    string TopicName,
    string? SubscriptionName = null,
    string? PushEndpoint = null)
{

    /// <summary>
    /// Pub/Sub topic name to which the message will be published.
    /// </summary>
    public string TopicName { get; init; } = TopicName;

    /// <summary>
    /// Subscription name associated with the topic.
    /// </summary>
    public string? SubscriptionName { get; init; } = SubscriptionName;

    /// <summary>
    /// (Optional) HTTP endpoint for push message delivery. If not provided, the subscription will be pull-based.
    /// </summary>
    public string? PushEndpoint { get; init; } = PushEndpoint;

    /// <summary>
    /// (Optional) Dead letter topic (DLQ) name where unprocessed messages will be forwarded after the maximum delivery attempts.
    /// If not provided, the subscription will not support dead lettering.
    /// </summary>
    public string? DeadLetterTopic
    {
        get; init;
    }

    /// <summary>
    /// (Optional) Maximum number of delivery attempts before forwarding the message to the dead letter topic.
    /// Only valid when <see cref="DeadLetterTopic"/> is defined. Default is 5.
    /// </summary>
    public int? MaxDeliveryAttempts
    {
        get; init;
    }

    /// <summary>
    /// (Optional) Acknowledgment deadline in seconds.
    /// If not provided, the default is 5 minutes.
    /// </summary>
    public int? AckDeadlineSeconds
    {
        get; init;
    }
}
