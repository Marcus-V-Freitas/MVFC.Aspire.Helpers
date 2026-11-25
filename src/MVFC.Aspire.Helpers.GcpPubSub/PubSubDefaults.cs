namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Default configuration constants for Google Pub/Sub.
/// </summary>
internal static class PubSubDefaults
{
    // Ports
    /// <summary>
    /// Default HTTP port of the Pub/Sub emulator.
    /// </summary>
    internal const int EMULATOR_PORT = 8681;

    /// <summary>
    /// Default HTTP port of the Pub/Sub administration UI.
    /// </summary>
    internal const int UI_PORT = 8680;

    // Docker images
    /// <summary>
    /// Default Pub/Sub emulator container image.
    /// </summary>
    internal const string EMULATOR_IMAGE = "thekevjames/gcloud-pubsub-emulator";

    /// <summary>
    /// Default Pub/Sub emulator container image tag.
    /// </summary>
    internal const string EMULATOR_IMAGE_TAG = "latest";

    /// <summary>
    /// Default Pub/Sub administration UI container image.
    /// </summary>
    internal const string UI_IMAGE = "echocode/gcp-pubsub-emulator-ui";

    /// <summary>
    /// Default Pub/Sub administration UI container image tag.
    /// </summary>
    internal const string UI_IMAGE_TAG = "latest";

    // Environment variable names
    /// <summary>
    /// Environment variable containing the emulator host and port.
    /// </summary>
    internal const string EMULATOR_HOST_ENV_VAR = "PUBSUB_EMULATOR_HOST";

    /// <summary>
    /// Environment variable controlling the emulator image initialization timeout.
    /// </summary>
    internal const string EMULATOR_WAIT_TIMEOUT_ENV_VAR = "PUBSUB_EMULATOR_WAIT_TIMEOUT";

    /// <summary>
    /// Prefix used for numbered project declarations passed to the emulator image.
    /// </summary>
    internal const string PROJECT_ENV_VAR_PREFIX = "PUBSUB_PROJECT";

    /// <summary>
    /// Environment variable containing the configured GCP project identifiers.
    /// </summary>
    internal const string GCP_PROJECT_IDS_ENV_VAR = "GCP_PROJECT_IDS";

    // Docker host for container-to-host communication (OS-aware)
    /// <summary>
    /// Docker host address used by containers to reach a consumer on the host operating system.
    /// </summary>
    internal static string DockerInternalHost =>
        OperatingSystem.IsLinux() ? "172.17.0.1" : "host.docker.internal";

    // Timeouts and limits
    /// <summary>
    /// Default acknowledgment deadline applied to subscriptions, in seconds.
    /// </summary>
    internal const int ACK_DEADLINE_SECONDS_DEFAULT = 300;

    /// <summary>
    /// Default maximum delivery attempts before sending a message to its dead letter topic.
    /// </summary>
    internal const int MAX_DELIVERY_ATTEMPTS_DEFAULT = 5;

    /// <summary>
    /// Default initialization timeout passed to the emulator image, in seconds.
    /// </summary>
    internal const int WAIT_TIMEOUT_SECONDS_DEFAULT = 15;

    /// <summary>
    /// Shared deadline for endpoint resolution and subscription configuration, in seconds.
    /// </summary>
    internal const int CONFIGURATION_TIMEOUT_SECONDS = 60;

    /// <summary>
    /// Maximum number of attempts for subscription reads during initialization.
    /// </summary>
    internal const int CONFIGURATION_MAX_ATTEMPTS = 30;

    // Delimiters for string construction
    /// <summary>
    /// Separator between project, topic and subscription declarations.
    /// </summary>
    internal const char CREATION_DELIMITER = ',';

    /// <summary>
    /// Separator between a topic name and its subscription name.
    /// </summary>
    internal const char SUBSCRIPTION_DELIMITER = ':';
}
