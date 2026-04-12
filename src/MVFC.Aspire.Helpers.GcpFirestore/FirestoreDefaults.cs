namespace MVFC.Aspire.Helpers.GcpFirestore;

/// <summary>
/// Default configuration constants for Google Firestore.
/// </summary>
internal static class FirestoreDefaults
{
    /// <summary>
    /// Default port used by the Firestore emulator.
    /// </summary>
    internal const int EMULATOR_PORT = 8080;

    /// <summary>
    /// Official Docker image for the Firestore emulator (via firebase-tools).
    /// </summary>
    internal const string EMULATOR_IMAGE = "mtlynch/firestore-emulator";

    /// <summary>
    /// Docker image tag for the Firestore emulator.
    /// </summary>
    internal const string EMULATOR_IMAGE_TAG = "latest";

    /// <summary>
    /// Environment variable name for the Firestore emulator host.
    /// </summary>
    public const string EMULATOR_HOST_ENV_VAR = "FIRESTORE_EMULATOR_HOST";

    /// <summary>
    /// Environment variable name for the delimited list of GCP Project IDs.
    /// </summary>
    public const string GCP_PROJECT_IDS_ENV_VAR = "Firestore__ProjectIds";

    /// <summary>
    /// Docker host address for container-to-host communication, OS-aware.
    /// Returns "172.17.0.1" for Linux, "host.docker.internal" for other OSes.
    /// </summary>
    internal static string DockerInternalHost =>
        OperatingSystem.IsLinux() ? "172.17.0.1" : "host.docker.internal";

    /// <summary>
    /// Default wait timeout in seconds for Firestore emulator startup.
    /// </summary>
    internal const int WAIT_TIMEOUT_SECONDS_DEFAULT = 15;

    /// <summary>
    /// Delimiter used for separating project IDs in environment variables.
    /// </summary>
    internal const char CREATION_DELIMITER = ',';
}