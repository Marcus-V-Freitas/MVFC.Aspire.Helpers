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
/// (Optional) HTTP route for push message delivery, combined with PushEndpointReference. If not provided, the subscription will be pull-based.
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
    /// (Optional) HTTP route for push message delivery, combined with PushEndpointReference. If not provided, the subscription will be pull-based.
    /// </summary>
    public string? PushEndpoint { get; init; } = PushEndpoint;

    /// <summary>
    /// Consumer endpoint used for this subscription's push delivery.
    /// Its allocated port is resolved at startup and combined with <see cref="PushEndpoint"/>.
    /// Required when a push route is configured; ignored for pull subscriptions.
    /// </summary>
    public EndpointReference? PushEndpointReference { get; init; }

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
    /// Applied only when <see cref="DeadLetterTopic"/> is defined. Accepts 5 through 100;
    /// null and zero use the default of 5.
    /// </summary>
    public int? MaxDeliveryAttempts
    {
        get; init;
    }

    /// <summary>
    /// (Optional) Acknowledgment deadline in seconds.
    /// Accepts 10 through 600 seconds. Null uses the library default of 5 minutes;
    /// zero requests the service default of 10 seconds.
    /// </summary>
    public int? AckDeadlineSeconds
    {
        get; init;
    }
}
