namespace MVFC.Aspire.Helpers.GcpPubSub.Models;

/// <summary>
/// Represents the Pub/Sub configuration for a project, including the project ID, message configurations (topics and subscriptions),
/// and the startup delay for resource initialization.
/// </summary>
public sealed record class PubSubConfig
{
    /// <summary>
    /// Initializes a new instance of <see cref="PubSubConfig"/> with a single <see cref="MessageConfig"/>.
    /// </summary>
    /// <param name="projectId">GCP project ID used by Pub/Sub.</param>
    /// <param name="messageConfig">Message configuration (topic and subscription) to be used.</param>
    /// <param name="secondsDelay">(Optional) Startup delay in seconds for resource initialization. Default: 5.</param>
    public PubSubConfig(string projectId, MessageConfig messageConfig, int secondsDelay = 5) : this(projectId, secondsDelay, [messageConfig])
    {

    }

    /// <summary>
    /// Initializes a new instance of <see cref="PubSubConfig"/> with multiple message configurations.
    /// </summary>
    /// <param name="projectId">GCP project ID used by Pub/Sub.</param>
    /// <param name="secondsDelay">(Optional) Startup delay in seconds for resource initialization. Default: 5.</param>
    /// <param name="messageConfigs">(Optional) List of message configurations (topics and subscriptions) to be used.</param>
    public PubSubConfig(string projectId, int secondsDelay = 5, IReadOnlyList<MessageConfig>? messageConfigs = null)
    {
        ProjectId = projectId;
        MessageConfigs = messageConfigs ?? [];
        UpDelay = TimeSpan.FromSeconds(secondsDelay);
    }

    /// <summary>
    /// GCP project ID used by Pub/Sub.
    /// </summary>
    public string ProjectId
    {
        get; init;
    }

    /// <summary>
    /// List of message configurations, each containing topic, subscription, and optional push endpoint information.
    /// </summary>
    public IReadOnlyList<MessageConfig> MessageConfigs
    {
        get; init;
    }

    /// <summary>
    /// Startup delay for Pub/Sub resource initialization.
    /// </summary>
    public TimeSpan UpDelay
    {
        get; init;
    }
}
