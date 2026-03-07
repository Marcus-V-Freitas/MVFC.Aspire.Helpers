namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Default configuration constants for Google Pub/Sub.
/// </summary>
internal static class PubSubDefaults
{
    // Ports
    internal const int EMULATOR_PORT = 8681;
    internal const int UI_PORT = 8680;

    // Docker images
    internal const string EMULATOR_IMAGE = "thekevjames/gcloud-pubsub-emulator";
    internal const string EMULATOR_IMAGE_TAG = "latest";
    internal const string UI_IMAGE = "echocode/gcp-pubsub-emulator-ui";
    internal const string UI_IMAGE_TAG = "latest";

    // Environment variable names
    internal const string EMULATOR_HOST_ENV_VAR = "PUBSUB_EMULATOR_HOST";
    internal const string EMULATOR_WAIT_TIMEOUT_ENV_VAR = "PUBSUB_EMULATOR_WAIT_TIMEOUT";
    internal const string PROJECT_ENV_VAR_PREFIX = "PUBSUB_PROJECT";
    internal const string GCP_PROJECT_IDS_ENV_VAR = "GCP_PROJECT_IDS";

    // Docker host for container-to-host communication (OS-aware)
    internal static string DockerInternalHost =>
        OperatingSystem.IsLinux() ? "172.17.0.1" : "host.docker.internal";

    // Timeouts and limits
    internal const int ACK_DEADLINE_SECONDS_DEFAULT = 300;
    internal const int MAX_DELIVERY_ATTEMPTS_DEFAULT = 5;
    internal const int WAIT_TIMEOUT_SECONDS_DEFAULT = 15;

    // Delimiters for string construction
    internal const char CREATION_DELIMITER = ',';
    internal const char SUBSCRIPTION_DELIMITER = ':';
}
